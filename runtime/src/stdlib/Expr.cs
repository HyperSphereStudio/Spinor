/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using runtime.core;
using runtime.core.Compilation;
using runtime.core.Utils;
using runtime.stdlib;
using runtime.Utils;

namespace Core;

public interface IExpr<TAny> {
    public Symbol Head { get; }
    public int ArgCount { get; }
    public TAny this[int i] { get; }
}

public class Expr : SystemAny, IExpr<Any>{
    public Symbol Head { get; }
    public List<Any> Args;
    
    public override Type Type => Types.Expr;
    
    public int ArgCount => Args.Count;

    public Any this[int i] => Args[i];
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
        TextWriter tw = new StringWriter();
        tw.Write("Expr(:");
        tw.Write(Head);
        tw.Write(", ");
        Args.Print(tw);
        tw.Write(")");
        return tw.ToString();
    }

    public void WriteCode(TextWriter tw) => new ExprExprWalker(tw).Walk(this);

    private class ExprExprWalker : ExprWalker {
        private readonly IndentedTextWriter _tw;
        public ExprExprWalker(TextWriter tw) => _tw = new(tw);

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
        public override void WalkStruct(bool isMutable, Symbol name, Expr block) {
            if(isMutable)
                _tw.Write("mutable");
            _tw.Write("struct ");
            WalkBlock(name, new(block, 0));
            _tw.Write("end");
        }
        public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {
            _tw.WriteLine(head);
            _tw.Indent++;
            foreach(var item in items)
                Walk(item);
            _tw.Indent--;
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
        public override void WalkSymbol(Symbol s) => _tw.Write(s);
        public override void WalkBool(bool b) => _tw.Write(b);
        public override void WalkInteger(long l) => _tw.Write(l);
        public override void WalkFloat(double d) => _tw.Write(d);
    }
}