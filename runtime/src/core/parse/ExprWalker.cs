/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Core;
using runtime.stdlib;

using static runtime.core.Spinor;

namespace runtime.core.parse;

public abstract class AbstractSpinorExprWalker : AbstractSpinorExprWalker<Any, Expr> {
   public override ExprType GetType(Any a) => a switch {
      Expr => ExprType.Expr,
      Symbol => ExprType.Symbol,
      LineNumberNode => ExprType.LineNumberNode,
      Any b => GetBoxedExprType(b.Type),
      _ => ExprType.Unknown
   };

   private static ExprType GetBoxedExprType(SType t) {
      if (t == Float64)
         return ExprType.Float;
      if (t == Int64)
         return ExprType.Integer;
      if (t == Bool)
         return ExprType.Bool;
      throw new SpinorException("Unknown Expr Type!");
   }

   public override Expr ToExpr(Any a) => (Expr) a;
   public override bool ToBool(Any a) => UnboxBool(a);
   public override void ToLineNumberNode(Any a, out int line, out string file) {
      var l = (LineNumberNode) a;
      line = l.Line;
      file = l.File;
   }
   
   public override Symbol ToSymbol(Any a) => (Symbol)a;
   public override long ToInteger(Any a) => UnboxInt64(a);
   public override double ToFloat(Any a) => Unbox<double>(a);
}