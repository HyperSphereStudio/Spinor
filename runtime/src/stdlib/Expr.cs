/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using runtime.core.Compilation;
using runtime.core.parse;
using runtime.core.type;
using runtime.stdlib;
using runtime.Utils;

namespace Core;

public interface IExpr<TAny> {
    public Symbol Head { get; }
    public int ArgCount { get; }
    public TAny this[int i] { get; }
}

public class Expr : IAny, IExpr<Any>{
    public Symbol Head { get; }
    public readonly List<Any> Args;
    public int ArgCount => Args.Count;
    public Any this[int i] => Args[i];
    public static SType RuntimeType { get; set; }
    public SType Type => RuntimeType;

    public Expr() => Args = new();
    public Expr(Symbol head, params Any[] body) {
        Head = head;
        Args = new(body);
    }
    public Expr(Symbol head, List<Any> body) {
        Head = head;
        Args = body;
    }

    public override string ToString() {
        StringWriter sw = new();
        Print(sw);
        return sw.ToString();
    }
    
    public void Print(TextWriter tw) {
        tw.Write("Expr(:");
        tw.Write(Head);
        tw.Write(", ");
        Args.Print(tw);
        tw.Write(")");
    }

    public void WriteCode(TextWriter tw) => new ExprExprWalker(tw).WalkExpr(this);
    private class ExprExprWalker : AbstractSpinorExprWalker {
        private readonly IndentedTextWriter _tw;

        public ExprExprWalker(TextWriter tw) => _tw = new(tw);
        
        public void WalkExpr(Any a, bool innerExpr = false) => Walk(a, innerExpr);
        public override void WalkCall(Symbol function, ExprArgWalker<Any, Expr> args) {
            _tw.Write(function);
            WalkTuple(args);
        }
        public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Expr> args) {
            if (op.Unary) {
                _tw.Write(op.Symbol);
                Walk(args[0], true);
            }else if(op.Binary) {
                if(innerExpr)
                    _tw.Write('(');
                while (args.MoveNext()) {
                    if (args.Index != args.StartIdx) {
                        _tw.Write(' ');
                        _tw.Write(op.Symbol);
                        _tw.Write(' ');
                    }
                    Walk(args.Current, true);
                }
                if(innerExpr)
                    _tw.Write(')');
            }
        }
        public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
            Walk(lhs);
            _tw.Write(' ');
            Walk(op.Symbol);
            if(op.Symbol != ASTSymbols.Assign)
                _tw.Write('=');
            _tw.Write(' '); 
            Walk(rhs);
        }
        public override void WalkModule(bool isBare, Symbol name, Expr block) {
            if(isBare)
                _tw.Write("bare");
            _tw.Write("module ");
            WalkBlock(name, new(block, 0));
            _tw.Write("end");
        }
        public override void WalkStruct(StructKind kind, Symbol name, Symbol extends, Expr block) {
            switch (kind) {
                case StructKind.Mutable:
                    _tw.Write("mutable");
                    break;
            }
            
            _tw.Write("struct ");
            WalkBlock(name, new(block, 0));
            _tw.Write("end");
        }
        
        public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {
            _tw.WriteLine(head == ASTSymbols.Block ? "begin" : head);
            _tw.Indent++;
            foreach(var item in items)
                Walk(item);
            _tw.Indent--;
            _tw.WriteLine();
            _tw.WriteLine("end");
        }
        public override void WalkTuple(ExprArgWalker<Any, Expr> items) {
            _tw.Write('(');
            while(items.MoveNext()) {
                if (items.Index != items.StartIdx) _tw.Write(", ");
                Walk(items.Current);
            }
            _tw.Write(')');
        }
        public override void WalkLineNumberNode(int line, string file) => _tw.WriteLine("\t#= " + line + ':' + file + "=#");
        public override void WalkAbstractType(Symbol name, Symbol extends, BuiltinType builtinType) {
            _tw.Write(builtinType == BuiltinType.None ? "abstract" : "abstractbuiltin");
            _tw.Write(" type ");
            _tw.Write(name);
            if (extends != null) {
                _tw.Write(" <: ");
                _tw.Write(extends);
            }
            _tw.Write(" end");
        }
        public override void WalkPrimitiveType(Symbol name, int bits, Symbol extends) {
            _tw.Write("primitive type ");
            _tw.Write(name);
            if (extends != null) {
                _tw.Write(" <: ");
                _tw.Write(extends);
            }
            _tw.Write(' ');
            _tw.Write(bits);
            _tw.Write(" end");
        }
        public override void WalkSymbol(Symbol s) => _tw.Write(s);
        public override void WalkBool(bool b) => _tw.Write(b);
        public override void WalkInteger(long l) => _tw.Write(l);
        public override void WalkFloat(double d) => _tw.Write(d);
    }
}