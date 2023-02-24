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

public interface ISpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny>{
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
    public virtual bool IsAbstractType(TExpr e) => e.Head == ASTSymbols.Abstract || IsBuiltinType(e);
    public virtual bool IsBuiltinType(TExpr e) => e.Head == ASTSymbols.Builtin;
    public virtual bool IsPrimitiveType(TExpr e) => e.Head == ASTSymbols.Primitive;
    public virtual bool IsExtend(TExpr e) => e.Head == ASTSymbols.Extends;
    public virtual bool IsSymbol(TAny a) => a is Symbol;
    
    public void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args);
    public void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args);
    public void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs);
    public void WalkModule(bool isBare, Symbol name, TExpr block);
    public void WalkStruct(bool isMutable, Symbol name, TExpr block);
    public void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items);
    public void WalkTuple(ExprArgWalker<TAny, TExpr> items);
    public void WalkLineNumberNode(int line, string file);
    public void WalkAbstractType(Symbol name, Symbol extends, bool isBuiltin);
    public void WalkPrimitiveType(Symbol name, int bits, Symbol extends);
    public TExpr ToExpr(TAny a);
    public bool ToBool(TAny a);
    public void ToLineNumberNode(TAny a, out int line, out string file);
    public Symbol ToSymbol(TAny a);
    public long ToInteger(TAny a);
    public double ToFloat(TAny a);
    public void WalkSymbol(Symbol s);
    public void WalkBool(bool b);
    public void WalkInteger(long l);
    public void WalkFloat(double d);
    public virtual void WalkUnknownExpr(TExpr e, bool innerExpr) => throw new SpinorException("Unknown Expression Head:" + e.Head);
    public virtual void WalkUnknownAny(TAny a, bool innerExpr) => throw new SpinorException("Unknown Any:" + a);
    
    
    public virtual void ParseCall(TExpr e) => WalkCall(ToSymbol(e[0]), new(e, 1));
    public virtual void ParseOperatorCall(TExpr e, SpinorOperator op, bool innerExpr) => WalkOperatorCall(innerExpr, op, new(e, 1));
    public virtual void ParseAssignment(TExpr e, SpinorOperator op) => WalkAssignment(op, e[0], e[1]);
    public virtual void ParseModule(TExpr e) => WalkModule(ToBool(e[0]), ToSymbol(e[1]), ToExpr(e[2]));
    public virtual void ParseStruct(TExpr e) => WalkStruct(ToBool(e[0]), ToSymbol(e[1]), ToExpr(e[2]));
    public virtual void ParseTuple(TExpr e) => WalkTuple(new(e, 0));
    public virtual void ParseBlock(TExpr e) => WalkBlock(e.Head, new(e, 0));
    public virtual void ParsePrimitiveType(TExpr e) {
        var bits = ToInteger(e[1]);
        Symbol name, extends = null;
        if (IsSymbol(e[0]))
            name = ToSymbol(e[0]);
        else{
            e = ToExpr(e[0]);
            name = ToSymbol(e[0]);
            extends = ToSymbol(e[1]);
        }
        WalkPrimitiveType(name, (int) bits, extends);
    }
    public virtual void ParseAbstractType(TExpr e, bool isBuiltIn) {
        Symbol name, extends = null;
        if (IsSymbol(e[0]))
            name = ToSymbol(e[0]);
        else{
            e = ToExpr(e[0]);
            name = ToSymbol(e[0]);
            extends = ToSymbol(e[1]);
        }
        WalkAbstractType(name, extends, isBuiltIn);
    }

    public virtual void WalkExpr(TExpr e, bool innerExpr) {
        if (IsCallExpr(e)) {
            if (IsOperatorCallExpr(e, out var op))
                ParseOperatorCall(e, op, innerExpr);
            else
                ParseCall(e);
        }
        else if(IsAssignmentExpr(e, out var sop))
            ParseAssignment(e, sop);
        else if(IsModule(e))
            ParseModule(e);
        else if(IsStruct(e))
            ParseStruct(e);
        else if(IsTuple(e))
            ParseTuple(e);
        else if(IsBlock(e))
            ParseBlock(e);
        else if (IsPrimitiveType(e))
            ParsePrimitiveType(e);
        else if (IsAbstractType(e))
            ParseAbstractType(e, IsBuiltinType(e));
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
    public ExprType GetType(TAny a);
}

public interface IImplSpinorWalker<TAny, TExpr> : ISpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny>{
    void ISpinorExprWalker<TAny, TExpr>.WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkModule(bool isBare, Symbol name, TExpr block)  => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkStruct(bool isMutable, Symbol name, TExpr block) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkTuple(ExprArgWalker<TAny, TExpr> items) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkLineNumberNode(int line, string file) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkAbstractType(Symbol name, Symbol extends, bool isBuiltin) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkPrimitiveType(Symbol name, int bits, Symbol extends) => throw new System.NotImplementedException(); 
    void ISpinorExprWalker<TAny, TExpr>.WalkSymbol(Symbol s) => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkBool(bool b)  => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkInteger(long l)  => throw new System.NotImplementedException();
    void ISpinorExprWalker<TAny, TExpr>.WalkFloat(double d)  => throw new System.NotImplementedException();
}