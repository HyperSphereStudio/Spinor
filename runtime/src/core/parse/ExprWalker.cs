/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Core;
using runtime.stdlib;

using static runtime.core.Spinor;

namespace runtime.core.parse;

public interface IExprWalker : ISpinorExprWalker<Any, Expr> {
   ExprType ISpinorExprWalker<Any, Expr>.GetType(Any a) => a switch {
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

   Expr ISpinorExprWalker<Any, Expr>.ToExpr(Any a) => (Expr) a;
   bool ISpinorExprWalker<Any, Expr>.ToBool(Any a) => UnboxBool(a);
   void ISpinorExprWalker<Any, Expr>.ToLineNumberNode(Any a, out int line, out string file) {
      var l = (LineNumberNode) a;
      line = l.Line;
      file = l.File;
   }
   
   Symbol ISpinorExprWalker<Any, Expr>.ToSymbol(Any a) => (Symbol)a;
   long ISpinorExprWalker<Any, Expr>.ToInteger(Any a) => UnboxInt64(a);
   double ISpinorExprWalker<Any, Expr>.ToFloat(Any a) => Unbox<double>(a);
}

public sealed class ExprWalker : IExprWalker, IImplSpinorWalker<Any, Expr> {}