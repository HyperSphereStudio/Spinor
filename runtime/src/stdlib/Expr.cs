using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using runtime.core;
using runtime.core.expr;
using runtime.Utils;

namespace Core;

public class Expr : SystemAny{
    public Symbol Head;
    public List<Any> Args;
    public override Type Type => Types.Expr;

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

    public void WriteCode(TextWriter tw) => WriteCode(this, new IndentedTextWriter(tw), false);

    private static void WriteCode(Any a, IndentedTextWriter tw, bool innerExpr) {
        switch (a) {
            case Expr e when e.Head == ASTSymbols.Call:
                e.WriteCall(tw, innerExpr);
                break;
            case Expr e when SpinorOperator.GetOp(e.Head == ASTSymbols.Assign ? 
                ASTSymbols.Assign : 
                e.Head.String[^1] == '=' ? 
                    (Symbol) e.Head.String.Substring(0, e.Head.String.Length - 1) 
                    : e.Head, out var sop) && sop.Assignable:
                e.WriteAssignment(sop, tw);
                break;
            case Expr e when e.Head == ASTSymbols.Module:
                e.WriteModule(tw);
                break;
            case Expr e when e.Head == ASTSymbols.Struct:
                e.WriteStruct(tw);
                break;
            case Expr e:
                throw new SpinorException("Unknown Expression Head:" + e.Head);
            case LineNumberNode n:
                tw.Write('\t');
                tw.WriteLine(n);
                break;
            default:
                tw.Write(a);
                break;
        }
    }
    private void WriteCall(IndentedTextWriter tw, bool innerExpr) {
        if(SpinorOperator.GetOp((Symbol) Args[0], out var sop)){
            if (sop.Unary) {
                tw.Write(Head);
                tw.Write(Args[0]);
            }else if(sop.Binary) {
                if(innerExpr)
                    tw.Write('(');
                for (int i = 1, n = Args.Count - 1; i < n; i++) {
                    WriteCode(Args[i], tw, true);
                    tw.Write(' ');
                    tw.Write(sop.Symbol);
                    tw.Write(' ');
                } 
                WriteCode(Args[^1], tw, true);
                if(innerExpr)
                    tw.Write(')');
            }
        }else{
            tw.Write(Args[0]);
            tw.Write('(');
            for (int i = 1, n = Args.Count, s = n - 1; i < n; i++) {
                if(i != s)
                    tw.Write(", ");
                WriteCode(Args[i], tw, false);
            }
            tw.Write(')');
        }
    }
    private void WriteAssignment(SpinorOperator so, IndentedTextWriter tw) {
        WriteCode(Args[0], tw, false);
        tw.Write(' ');
        tw.Write(so.Symbol);
        tw.Write(' ');
        WriteCode(Args[1], tw, false);
    }
    private void WriteModule(IndentedTextWriter tw) {
        if(Spinor.Unbox<bool>(Args[0]))
            tw.Write("bare");
        tw.Write("module ");
        tw.WriteLine(Args[1].String());
        ((Expr) Args[2]).WriteBlock(tw, false);
        tw.Write("end");
    }
    private void WriteStruct(IndentedTextWriter tw) {
        if(Spinor.Unbox<bool>(Args[0]))
            tw.Write("mutable");
        tw.Write("struct ");
        tw.WriteLine(Args[1].String());
        ((Expr) Args[2]).WriteBlock(tw, false);
        tw.Write("end");
    }
    private void WriteBlock(IndentedTextWriter tw, bool showHead) {
        if(showHead)
            tw.WriteLine(Head);
        tw.Indent++;
        foreach(var line in Args)
            WriteCode(line, tw, false);
        tw.Indent--;
    }
}