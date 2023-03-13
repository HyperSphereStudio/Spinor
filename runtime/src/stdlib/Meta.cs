/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using runtime.core.compilation.Expr;

namespace Core;

public static class Meta
{
   public static ILoweredExpr Lower(CompileTimeModule m, Any expr) => new ExprLowerer(m).Lower(expr);

}