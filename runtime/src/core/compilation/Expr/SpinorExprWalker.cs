/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using runtime.core;
using runtime.core.Compilation;
using runtime.core.type;

namespace runtime.stdlib;

public enum ExprType : byte{
    Expr,
    
    LineNumberNode,
    Symbol,
    Bool,
    Integer,
    Float,
    Unknown,
    
    LiteralStart = LineNumberNode,
    LiteralEnd = Float
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
    public ExprArgWalker(ExprArgWalker<TAny, TExpr> expr, int start = 0) : this(expr.Expr, start + expr.StartIdx){}
    public ExprArgWalker(ExprArgWalker<TAny, TExpr> expr, int start, int stop) : this(expr.Expr, start + expr.StartIdx, stop){}

    public TAny this[int i] => Expr[i + StartIdx];
    
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

public abstract class AbstractSpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny> {
    public virtual bool HeadEquals(TExpr e, Symbol h) => e.Head == h;
    public virtual bool HeadEquals(TExpr e, params Symbol[] hs) {
        foreach(var h in hs)
            if (HeadEquals(e, h))
                return true;
        return false;
    }
    public virtual bool IsCallExpr(TExpr e) => HeadEquals(e, ASTSymbols.Call);
    public virtual bool IsOperatorCallExpr(TExpr e, out SpinorOperator op) => SpinorOperator.GetOp(ToSymbol(e[0]), out op);
    public virtual bool IsAssignmentExpr(TExpr e, out SpinorOperator sop) =>
        SpinorOperator.GetOp(e.Head == ASTSymbols.Assign ? ASTSymbols.Assign
            : e.Head.String[^1] == '=' ? (Symbol) e.Head.String.Substring(0, e.Head.String.Length - 1)
            : e.Head, out sop) && sop.Assignable;
    public virtual bool IsModule(TExpr e) => HeadEquals(e, ASTSymbols.Module);
    public virtual bool IsStruct(TExpr e) => HeadEquals(e, ASTSymbols.Struct);
    public virtual bool IsTuple(TExpr e) => HeadEquals(e, ASTSymbols.Tuple);
    public virtual bool IsBlock(TExpr e) => HeadEquals(e, ASTSymbols.Quote, ASTSymbols.Block);
    public virtual bool IsBuiltinType(TExpr e) => HeadEquals(e, ASTSymbols.Builtin);
    public virtual bool IsAbstractType(TExpr e) => HeadEquals(e, ASTSymbols.Abstract) || IsBuiltinType(e);
    public virtual bool IsPrimitiveType(TExpr e) => HeadEquals(e, ASTSymbols.Primitive);
    public virtual bool IsExtend(TExpr e) => HeadEquals(e, ASTSymbols.Extends);
    public virtual bool IsSymbol(TAny a) => a is Symbol;
    public bool IsLiteral(ExprType type) => type is >= ExprType.LiteralStart and <= ExprType.LiteralEnd;

    public abstract void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs);
    public abstract void WalkModule(bool isBare, Symbol name, TExpr block);
    public abstract void WalkStruct(StructKind kind, Symbol name, Symbol extends, TExpr block);
    public abstract void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkTuple(ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkLineNumberNode(int line, string file);
    public abstract void WalkAbstractType(Symbol name, Symbol extends, BuiltinType builtinType);
    public abstract void WalkPrimitiveType(Symbol name, int bits, Symbol extends);
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
    public abstract ExprType GetType(TAny a);
    public virtual void WalkUnknownExpr(TExpr e, bool innerExpr) => ThrowParsingError($"Unknown Expression Head:{e.Head}");
    public virtual void WalkUnknownAny(TAny a, bool innerExpr) => ThrowParsingError($"Unknown Any:{a}");
    public virtual void ThrowParsingError(string msg, string help = null) => throw new SpinorException(msg);
    
    public virtual void ParseCall(TExpr e) => WalkCall(ToSymbol(e[0]), new(e, 1));
    public virtual void ParseOperatorCall(TExpr e, SpinorOperator op, bool innerExpr) => WalkOperatorCall(innerExpr, op, new(e, 1));
    public void ParseAssignment(TExpr e, SpinorOperator op) => WalkAssignment(op, e[0], e[1]);
    public void ParseModule(TExpr e) => WalkModule(ToBool(e[0]), ToSymbol(e[1]), ToExpr(e[2]));

    public void ParseStruct(TExpr e) {
        var blk = ParseSignature(new(e, 1), out var name, out var extends);
        WalkStruct(ToBool(e[0]) ? StructKind.Mutable : StructKind.Struct, name, extends, ToExpr(blk[0]));
    }
    
    public virtual void ParseTuple(TExpr e) => WalkTuple(new(e, 0));
    public virtual void ParseBlock(TExpr e) => WalkBlock(e.Head, new(e, 0));

    public void ParseName(TAny a, out Symbol name, out Symbol type) {
        if (IsSymbol(a)) {
            name = ToSymbol(a);
            type = default;
            return;
        }
        
        var e = ToExpr(a);
        if(!HeadEquals(e, ASTSymbols.Colon))
            ThrowParsingError("Name Doesnt Use Colon Convention for Type Extending!");
        
        name = ToSymbol(e[0]);
        type = ToSymbol(e[1]);
    }

    public virtual ExprArgWalker<TAny, TExpr> ParseSignature(ExprArgWalker<TAny, TExpr> e, out Symbol name, out Symbol extends) {
        if (IsSymbol(e[0])) {
            name = ToSymbol(e[0]);
            extends = default;
            return new(e, 1);
        }
        
        var e2 = ToExpr(e[0]);
        name = ToSymbol(e2[0]);
        extends = ToSymbol(e2[1]);

        return new(e, 1);
    }
    
    public void ParsePrimitiveType(TExpr e) {
        var expr = ParseSignature(new(e), out var name, out var extends);
        var bits = ToInteger(expr[0]);
        WalkPrimitiveType(name, (int) bits, extends);
    }
    
    public void ParseAbstractType(TExpr e, bool isBuiltIn) {
        ParseSignature(new(e, 0),  out var name, out var extends);
        var ty = BuiltinType.None;
        if (isBuiltIn) {
            if (name == CommonSymbols.AbstractFloat)
                ty = BuiltinType.FloatingNumber;
            else if (name == CommonSymbols.Signed)
                ty = BuiltinType.SignedInteger;
            else if (name == CommonSymbols.Unsigned)
                ty = BuiltinType.UnsignedInteger;
            else if (name == CommonSymbols.Exception)
                ty = BuiltinType.Exception;
            else ThrowParsingError("No Builtin Symbol Found!");
        }
        WalkAbstractType(name, extends, ty);
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

    public virtual void WalkLiteral(TAny a, ExprType ty) {
        switch (ty) {
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
            default:
                throw new SpinorException("Error In Parsing!");
        }
    }

    public void Walk(TAny a, bool innerExpr = false) {
        var ty = GetType(a);
        if (IsLiteral(ty))
            WalkLiteral(a, ty);
        else {
            switch (ty) {
                case ExprType.Expr:
                    WalkExpr(ToExpr(a), innerExpr);
                    break;
                case ExprType.Unknown:
                default:
                    WalkUnknownAny(a, innerExpr);
                    break;
            }
        }
    }
}