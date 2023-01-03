/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Core;
using runtime.stdlib;

namespace runtime.core.Compilation;

public abstract class ExprWalker : SpinorExprWalker<Any, Expr> {
   public override ExprType GetType(Any a) => a switch {
      Expr => ExprType.Expr,
      Symbol => ExprType.Symbol,
      LineNumberNode => ExprType.LineNumberNode,
      SystemPrimitiveAny<bool> => ExprType.Bool,
      SystemPrimitiveAny<double> => ExprType.Float,
      SystemPrimitiveAny<long> => ExprType.Integer,
      _ => ExprType.Unknown
   };
    
   public override Expr ToExpr(Any a) => (Expr) a;
   public override bool ToBool(Any a) => Spinor.UnboxBool(a);
   public override void ToLineNumberNode(Any a, out int line, out string file) {
      var l = (LineNumberNode) a;
      line = l.Line;
      file = l.File;
   }
   public override Symbol ToSymbol(Any a) => (Symbol)a;
   public override long ToInteger(Any a) => Spinor.UnboxInt64(a);
   public override double ToFloat(Any a) => Spinor.Unbox<double>(a);
}

public class ImplementedExprWalker : ExprWalker {
   public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Expr> args) => throw new System.NotImplementedException();
   public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs)  => throw new System.NotImplementedException();
   public override void WalkModule(bool isBare, Symbol name, Expr block)  => throw new System.NotImplementedException();
   public override void WalkSymbol(Symbol s)  => throw new System.NotImplementedException();
   public override void WalkBool(bool b) => throw new System.NotImplementedException();
   public override void WalkInteger(long l) => throw new System.NotImplementedException();
   public override void WalkFloat(double d)  => throw new System.NotImplementedException();
   public override void WalkStruct(bool isMutable, Symbol name, Expr block) => throw new System.NotImplementedException();
   public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) => throw new System.NotImplementedException();
   public override void WalkCall(Symbol function, ExprArgWalker<Any, Expr> args) => throw new System.NotImplementedException();
   public override void WalkLineNumberNode(int line, string file) => throw new System.NotImplementedException();
   public override void WalkTuple(ExprArgWalker<Any, Expr> items) => throw new System.NotImplementedException();
}