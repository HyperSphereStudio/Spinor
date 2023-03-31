/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections;
using System.Collections.Generic;
using runtime.core.math;
using runtime.stdlib;

namespace runtime.core.compilation.interp;
using static SpinorTypeAttributes;

public enum ExprType : byte {
    Expr,
    Symbol,
    
    LineNumberNode,
    Bool,
    Integer,
    Float,
    Nothing,
    Unknown,

    LiteralStart = LineNumberNode,
    LiteralEnd = Float
}

public struct ExprArgWalker<TAny, TExpr> : IEnumerator<TAny>, IEnumerable<TAny> where TExpr : IExpr<TAny> {
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

    public bool MoveNext()
    {
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
    public delegate void ExprParser(AbstractSpinorExprWalker<TAny, TExpr> ins, TExpr ex);

    #region TrivialTable
    public readonly Dictionary<Symbol, ExprParser> TrivialParserHeadTable = new() {
        {
            ASTSymbols.Call, (w, x) => {
                if (SpinorOperator.GetOp(w.ToSymbol(x[0]), out var op))
                    w.WalkOperatorCall(op, new(x, 1));
                else
                    w.WalkCall(w.ToExpr(x[0]), new(x, 1));
            }
        },

        {ASTSymbols.Module, (w, x) => w.WalkModule(w.ToBool(x[0]), w.ToSymbol(x[1]), w.ToExpr(x[2])) },
        {ASTSymbols.Struct, (w, x) => w.WalkStruct(w.ToBool(x[0]) ? Mutable|Class : 0, w.ToSymbol(x[1]), x[2], w.ToExpr(x[3]))},
        {ASTSymbols.Tuple, (w, x) => w.WalkTuple(new(x)) },
        {ASTSymbols.Quote, (w, x) => w.WalkQuote(new(x))},
        {ASTSymbols.Block, (w, x) => w.WalkBlock(x.Head, new(x)) },
        {ASTSymbols.Abstract, (w, x) => w.WalkAbstractType(w.ToSymbol(x[0]), x[1])},
        {ASTSymbols.Using, (w, x) => w.WalkUsing(w.ToBool(x[0]), new(x, 1))},

    };
    
    #endregion
    #region ComplexVerification
    public bool HeadEquals(TExpr e, Symbol h) => e.Head == h;
    public bool IsAssignmentExpr(TExpr e, out SpinorOperator sop) =>
        SpinorOperator.GetOp(e.Head == ASTSymbols.Assign ? ASTSymbols.Assign
            : e.Head.String[^1] == '=' ? (Symbol) e.Head.String[..^1]
            : e.Head, out sop) && sop.Assignable;

    public bool IsSymbol(TAny a) => GetType(a) == ExprType.Symbol;
    public bool IsNothing(TAny a) => GetType(a) == ExprType.Nothing;
    public bool IsLiteral(ExprType type) => type is >= ExprType.LiteralStart and <= ExprType.LiteralEnd;
    #endregion
    #region AbstractWalkers
    public abstract void WalkName(Symbol s);
    public abstract void WalkSymbol(Symbol s);
    public abstract void WalkNothing();
    public abstract void WalkBool(bool b);
    public abstract void WalkInteger(long l);
    public abstract void WalkFloat(double d);
    public abstract void WalkCall(TExpr function, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkOperatorCall(SpinorOperator op, ExprArgWalker<TAny, TExpr> args);
    public abstract void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs);
    public abstract void WalkModule(bool isBare, Symbol name, TExpr block);
    public abstract void WalkStruct(SpinorTypeAttributes structType, Symbol name, TAny extends, TExpr block);
    public abstract void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkTuple(ExprArgWalker<TAny, TExpr> items);
    public abstract void WalkLineNumberNode(int line, string file);
    public abstract void WalkAbstractType(Symbol name, TAny extends);
    public abstract void WalkUsing(bool isSystem, ExprArgWalker<TAny, TExpr> args);
    #endregion
    #region ImplWalkers
    public virtual void WalkUnknownExpr(TExpr e) => ThrowParsingError($"Unknown Expression Head:{e.Head}");
    public virtual void WalkUnknownAny(TAny a) => ThrowParsingError($"Unknown Any:{a}");
    public virtual void WalkExpr(TExpr e) {
        try {
            if (TrivialParserHeadTable.TryGetValue(e.Head, out var f))
                f(this, e);
            else {
                if (IsAssignmentExpr(e, out var sop))
                    ParseAssignment(e, sop);
                else
                    WalkUnknownExpr(e);
            }
        }
        catch (SpinorException) {
            throw;
        }catch (Exception ex) {
            throw new SpinorException($"Encountered Error While Parsing {e}", ex);
        }
    }
    public virtual void WalkLiteral(TAny a, ExprType ty) {
        switch (ty) {
            case ExprType.LineNumberNode:
                ToLineNumberNode(a, out var line, out var file);
                WalkLineNumberNode(line, file);
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
            case ExprType.Nothing:
                WalkNothing();
                break;
            default:
                ThrowParsingError("No Literal{a} Found!");
                break;
        }
    }
    
    public virtual void Walk(TAny a) {
        var ty = GetType(a);
        if (IsLiteral(ty))
            WalkLiteral(a, ty);
        else {
            switch (ty) {
                case ExprType.Expr:
                    WalkExpr(ToExpr(a));
                    break;
                case ExprType.Symbol:
                    WalkName(ToSymbol(a));
                    break;
                case ExprType.Nothing:
                    WalkNothing();
                    break;
                default:
                    WalkUnknownAny(a);
                    break;
            }
        }
    }

    public virtual void WalkQuote(ExprArgWalker<TAny, TExpr> n) {
        foreach (var k in n) {
            var ty = GetType(k);
            switch (ty) {
                case ExprType.Symbol:
                    WalkSymbol(ToSymbol(k));
                    break;
                default:
                    ThrowParsingError("Unknown Quote {k}");
                    break;
            }
        }    
    }
    
    #endregion
    #region Conversion
    public abstract TExpr ToExpr(TAny a);
    public abstract bool ToBool(TAny a);
    public abstract void ToLineNumberNode(TAny a, out int line, out string file);
    public abstract Symbol ToSymbol(TAny a);
    public abstract long ToInteger(TAny a);
    public abstract double ToFloat(TAny a);
    public abstract string ToString(TAny a);
    #endregion
    #region ComplexParsers
    public virtual void ParseAssignment(TExpr e, SpinorOperator op) => WalkAssignment(op, e[0], e[1]);
    #endregion
    
    public abstract ExprType GetType(TAny a);
    public virtual void ThrowParsingError(string msg, string help = null) => throw new SpinorException(msg);
   
}