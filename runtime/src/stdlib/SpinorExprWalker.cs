/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.Collections;
using System.Collections.Generic;
using Core;
using runtime.core;
using runtime.core.Compilation;

namespace runtime.stdlib;

public enum ExprType : byte{
    Expr,
    LineNumberNode,
    Symbol,
    Bool,
    Integer,
    Float,
    Unknown
}

public struct ExprArgWalker <TAny, TExpr> : IEnumerator<TAny>, IEnumerable<TAny> where TExpr : IExpr<TAny> {
    public readonly TExpr Expr;
    public readonly int StartIdx, StopIndex;
    public int Index;
    public int Length => StopIndex - StartIdx;

    public ExprArgWalker(TExpr expr, int start, int stop) {
        StartIdx = start;
        StopIndex = stop;
        Index = -1;
        Expr = expr;
    }
    
    public ExprArgWalker(TExpr expr, int start = 0) : this(expr, start, expr.ArgCount){}

    public TAny this[int i] => Expr[i];
    
    public bool MoveNext() {
        if (Index == -1)
            Index = StartIdx - 1;
        return ++Index < StopIndex;
    }
    public void Reset() => Index = -1;
    public TAny Current => this[Index];
    object IEnumerator.Current => Current;
    public void Dispose(){}
    public IEnumerator<TAny> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;
}

public abstract class SpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny>{
    public virtual bool IsCallExpr(TExpr e) => e.Head == ASTSymbols.Call;
    public virtual bool IsOperatorCallExpr(TExpr e, out SpinorOperator op) => SpinorOperator.GetOp(ToSymbol(e[0]), out op);
    public virtual bool IsAssignmentExpr(TExpr e, out SpinorOperator sop) =>
        SpinorOperator.GetOp(e.Head == ASTSymbols.Assign ? ASTSymbols.Assign
            : e.Head.String[^1] == '=' ? (Symbol) e.Head.String.Substring(0, e.Head.String.Length - 1)
            : e.Head, out sop) && sop.Assignable;
    public virtual bool IsModule(TExpr e) => e.Head == ASTSymbols.Module;
    public virtual bool IsStruct(TExpr e) => e.Head == ASTSymbols.Struct;
    public virtual bool IsTuple(TExpr e) => e.Head == ASTSymbols.Tuple;
    public virtual bool IsBlock(TExpr e) => e.Head == ASTSymbols.Quote || e.Head == ASTSymbols.Block;
    
    public abstract void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs);
    
    public abstract void WalkModule(bool isBare, Symbol name, TExpr block);
    public abstract void WalkStruct(bool isMutable, Symbol name, TExpr block);
    public abstract void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkTuple(ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkLineNumberNode(int line, string file);
    public abstract TExpr ToExpr(TAny a);
    public abstract bool ToBool(TAny a);
    public abstract void ToLineNumberNode(TAny a, out int line, out string file);
    public abstract Symbol ToSymbol(TAny a);
    public abstract long ToInteger(TAny a);
    public abstract double ToFloat(TAny a);
    public abstract void WalkSymbol(Symbol s);
    public abstract void WalkBool(bool b);
    public abstract void WalkInteger(long l);
    public abstract void WalkFloat(double d);
    public virtual void WalkUnknownExpr(TExpr e, bool innerExpr) => throw new SpinorException("Unknown Expression Head:" + e.Head);
    public virtual void WalkUnknownAny(TAny a, bool innerExpr) => throw new SpinorException("Unknown Any:" + a);
    public virtual void WalkExpr(TExpr e, bool innerExpr) {
        if (IsCallExpr(e)) {
            if(IsOperatorCallExpr(e, out var op))
                WalkOperatorCall(innerExpr, op, new(e, 1));
            else 
                WalkCall(ToSymbol(e[0]), new(e, 1));
        }
        else if(IsAssignmentExpr(e, out var sop))
            WalkAssignment(sop, e[0], e[1]);
        else if(IsModule(e))
            WalkModule(ToBool(e[0]), ToSymbol(e[1]), ToExpr(e[2]));
        else if(IsStruct(e))
            WalkStruct(ToBool(e[0]), ToSymbol(e[1]), ToExpr(e[2]));
        else if(IsTuple(e))
            WalkTuple(new(e, 0));
        else if(IsBlock(e))
            WalkBlock(e.Head, new(e, 0));
        else 
            WalkUnknownExpr(e, innerExpr);
    }
    public virtual void Walk(TAny a, bool innerExpr = false) {
        var ty = GetType(a);
        switch (ty) {
            case ExprType.Expr:
                WalkExpr(ToExpr(a), innerExpr);
                break;
            case ExprType.LineNumberNode:
                ToLineNumberNode(a, out var line, out var file);
                WalkLineNumberNode(line, file);
                break;
            case ExprType.Symbol:
                WalkSymbol(ToSymbol(a));
                break;
            case ExprType.Bool:
                WalkBool(ToBool(a));
                break;
            case ExprType.Float:
                WalkFloat(ToFloat(a));
                break;
            case ExprType.Integer:
                WalkInteger(ToInteger(a));
                break;
            case ExprType.Unknown:
            default:
                WalkUnknownAny(a, innerExpr);
                break;
        }
    }
    public abstract ExprType GetType(TAny a);
}