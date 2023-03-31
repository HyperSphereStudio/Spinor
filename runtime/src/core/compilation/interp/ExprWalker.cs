/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using Core;
using runtime.core.internals;
using runtime.stdlib;
using static runtime.core.internals.Spinor;

namespace runtime.core.compilation.interp;

public abstract class AbstractSpinorExprWalker : AbstractSpinorExprWalker<Any, Expr> {
   public override ExprType GetType(Any a) {
      try {
         if (a.IsNothing)
            return ExprType.Nothing;
         return a.Value switch {
            null => ExprType.Nothing,
            Expr => ExprType.Expr,
            Symbol => ExprType.Symbol,
            LineNumberNode => ExprType.LineNumberNode,
            ISystemAny b => GetBoxedExprType(b.Type),
            _ => ExprType.Unknown
         };
      }
      catch (Exception) {
         Console.Error.WriteLine($"Caught Error While Getting Type of {a}::{a.GetType()}");
         throw;
      }
   }

   private static ExprType GetBoxedExprType(SType t) {
      if (t == Float64)
         return ExprType.Float;
      if (t == Spinor.Int64)
         return ExprType.Integer;
      if (t == Bool)
         return ExprType.Bool;
      throw new SpinorException($"Unknown Expr Type! {t}");
   }

   public override Expr ToExpr(Any a) => a.IsNothing ? null : a;
   public override bool ToBool(Any a) => (bool) a;
   public override void ToLineNumberNode(Any a, out int line, out string file) {
      var l = (LineNumberNode) a;
      line = l.Line;
      file = l.File;
   }
   
   public override Symbol ToSymbol(Any a) => a;
   public override long ToInteger(Any a) => (long) a;
   public override double ToFloat(Any a) => (double) a;
   public override string ToString(Any a) => a.ToString();
}