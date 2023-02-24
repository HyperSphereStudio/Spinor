/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using runtime.core;
using runtime.core.Compilation;
using runtime.core.memory;
using runtime.core.parse;
using runtime.stdlib;
using runtime.Utils;

namespace Core;

public interface IExpr<TAny> {
    public Symbol Head { get; }
    public int ArgCount { get; }
    public TAny this[int i] { get; }
}

public sealed class Expr : IAny<Expr>, IExpr<Any>{
    public Symbol Head { get; }
    public readonly List<Any> Args;
    public int ArgCount => Args.Count;
    public Any this[int i] => Args[i];
    public static SType RuntimeType { get; set; }
    public Expr This => this;
    
    void Any.Serialize(SpinorSerializer serializer) {
        throw new SpinorException();
    }

    public Expr() => Args = new();
    public Expr(Symbol head, params Any[] body) {
        Head = head;
        Args = new(body);
    }
    public Expr(Symbol head, List<Any> body) {
        Head = head;
        Args = body;
    }
    
    public void Print(TextWriter tw) {
        tw.Write("Expr(:");
        tw.Write(Head);
        tw.Write(", ");
        Args.Print(tw);
        tw.Write(")");
    }

    public void WriteCode(TextWriter tw) => new ExprExprWalker(tw).WalkExpr(this);
    private class ExprExprWalker : IExprWalker {
        private readonly IndentedTextWriter _tw;
        private IExprWalker This => this;
        
        public ExprExprWalker(TextWriter tw) => _tw = new(tw);
        
        public void WalkExpr(Any a, bool innerExpr = false) => This.Walk(a, innerExpr);
        public void WalkCall(Symbol function, ExprArgWalker<Any, Expr> args) {
            _tw.Write(function);
            WalkTuple(args);
        }
        public void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Expr> args) {
            if (op.Unary) {
                _tw.Write(op.Symbol);
                This.Walk(args[0], true);
            }else if(op.Binary) {
                if(innerExpr)
                    _tw.Write('(');
                while (args.MoveNext()) {
                    if (args.Index != args.StartIdx) {
                        _tw.Write(' ');
                        _tw.Write(op.Symbol);
                        _tw.Write(' ');
                    }
                    This.Walk(args.Current, true);
                }
                if(innerExpr)
                    _tw.Write(')');
            }
        }
        public void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
            This.Walk(lhs);
            _tw.Write(' ');
            This.Walk(op.Symbol);
            if(op.Symbol != ASTSymbols.Assign)
                _tw.Write('=');
            _tw.Write(' ');
            This.Walk(rhs);
        }
        public void WalkModule(bool isBare, Symbol name, Expr block) {
            if(isBare)
                _tw.Write("bare");
            _tw.Write("module ");
            WalkBlock(name, new(block, 0));
            _tw.Write("end");
        }
        public void WalkStruct(bool isMutable, Symbol name, Expr block) {
            if(isMutable)
                _tw.Write("mutable");
            _tw.Write("struct ");
            WalkBlock(name, new(block, 0));
            _tw.Write("end");
        }
        
        public void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {
            _tw.WriteLine(head == ASTSymbols.Block ? "begin" : head);
            _tw.Indent++;
            foreach(var item in items)
                This.Walk(item);
            _tw.Indent--;
            _tw.WriteLine();
            _tw.WriteLine("end");
        }
        public void WalkTuple(ExprArgWalker<Any, Expr> items) {
            _tw.Write('(');
            while(items.MoveNext()) {
                if (items.Index != items.StartIdx) _tw.Write(", ");
                This.Walk(items.Current);
            }
            _tw.Write(')');
        }
        public void WalkLineNumberNode(int line, string file) => _tw.WriteLine("\t#= " + line + ':' + file + "=#");
        public void WalkAbstractType(Symbol name, Symbol extends, bool isBuiltin) {
            _tw.Write(isBuiltin ? "builtin" : "abstract");
            _tw.Write(" type ");
            _tw.Write(name);
            if (extends != null) {
                _tw.Write(" <: ");
                _tw.Write(extends);
            }
            _tw.Write(" end");
        }
        public void WalkPrimitiveType(Symbol name, int bits, Symbol extends) {
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
        public void WalkSymbol(Symbol s) => _tw.Write(s);
        public void WalkBool(bool b) => _tw.Write(b);
        public void WalkInteger(long l) => _tw.Write(l);
        public void WalkFloat(double d) => _tw.Write(d);
    }
}