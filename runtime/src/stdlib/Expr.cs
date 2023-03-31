/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using runtime.core.compilation.interp;
using runtime.core.internals;
using runtime.core.math;

namespace runtime.stdlib;

public interface IExpr<out TAny> {
    public Symbol Head { get; }
    public int ArgCount { get; }
    public TAny this[int i] { get; }
}

public class Expr : ISpinorAny, IExpr<Any>{
    public Symbol Head { get; }
    public readonly List<Any> Args;
    public int ArgCount => Args.Count;
    public Any this[int i] => Args[i];
    
    public static SType RuntimeType { get; } = 
        Spinor.Core.InitializeStructType(SpinorTypeAttributes.Class, "Expr", 
            Any.RuntimeType, 1, null, 
            new TypeLayout(0, typeof(Expr), new SpinorField[2], null, null));
    
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

    void IAny.Print(TextWriter tw) {
        tw.Write("Expr(:");
        tw.Write(Head);
        tw.Write(", Any[");
        for (var i = 0; i < Args.Count; i++) {
            if(i != 0)
                tw.Write(", ");
            Args[i].Print(tw);
        }
        tw.Write("])");
    }

    public override string ToString() => ((IAny) this).String();
    public static implicit operator Any(Expr e) => new(e);
    public static implicit operator Expr(Any a) => a.Cast<Expr>();
    
    public void WriteCode(TextWriter tw) => new ExprExprWalker(tw).WalkExpr(this);
    private class ExprExprWalker : AbstractSpinorExprWalker {
        private readonly IndentedTextWriter _tw;
        private bool _innerExpr;

        public ExprExprWalker(TextWriter tw) => _tw = new(tw);
        
        public void WalkExpr(Any a) => Walk(a);
        public override void WalkCall(Expr function, ExprArgWalker<Any, Expr> args) {
            Walk(function);
            WalkTuple(args);
        }
        public override void WalkOperatorCall(SpinorOperator op, ExprArgWalker<Any, Expr> args) {
            var isInnerExpr = _innerExpr;
            if (op.Unary) {
                _tw.Write(op.Symbol);
                _innerExpr = true;
                Walk(args[0]);
            }else if(op.Binary) {
                if(isInnerExpr)
                    _tw.Write('(');
                while (args.MoveNext()) {
                    if (args.Index != args.StartIdx) {
                        _tw.Write(' ');
                        _tw.Write(op.Symbol);
                        _tw.Write(' ');
                    }
                    _innerExpr = true;
                    Walk(args.Current);
                }
                if(isInnerExpr)
                    _tw.Write(')');
            }
            _innerExpr = isInnerExpr;
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
        public override void WalkStruct(SpinorTypeAttributes type, Symbol name, Any extends, Expr block) {
            if(type.HasFlag(SpinorTypeAttributes.Mutable))
                _tw.Write("mutable");
            
            _tw.Write("struct ");

            if (!extends.IsNothing) {
                _tw.Write(":<");
                Walk(extends);
            }

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
        public override void WalkAbstractType(Symbol name, Any extends) {
            _tw.Write("abstract ");
            _tw.Write("type ");
            _tw.Write(name);
            if (!extends.IsNothing) {
                _tw.Write(" <: ");
                Walk(extends);
            }
            _tw.Write(" end");
        }

        public override void WalkUsing(bool isSystem, ExprArgWalker<Any, Expr> n) {
            if(isSystem)
                _tw.Write("system ");
            _tw.Write("using ");
            Walk(n.Expr);
        }

        public override void WalkName(Symbol s) => _tw.Write(s);
        public override void WalkSymbol(Symbol s) => _tw.Write(s);
        public override void WalkNothing() => _tw.Write("nothing");
        public override void WalkBool(bool b) => _tw.Write(b);
        public override void WalkInteger(long l) => _tw.Write(l);
        public override void WalkFloat(double d) => _tw.Write(d);
    }
}