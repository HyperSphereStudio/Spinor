/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using Core;
using runtime.core.Compilation;
using runtime.core.parse;
using runtime.core.type;
using runtime.stdlib;

namespace runtime.core.compilation.Expr;

public interface ILoweredExpr
{
   public virtual SType Type => Any.RuntimeType;
}

public record Block() : ILoweredExpr;
public record Literal(Any Value, ExprType Type) : ILoweredExpr;

internal class ExprLowerer : AbstractSpinorExprWalker {
   public ILoweredExpr LastExpr;
   private readonly RuntimeModule _Module;
   
   internal ExprLowerer(RuntimeModule module) {
      _Module = module;
   }

   public ILoweredExpr Lower(Any a) {
      var keep = LastExpr;
      LastExpr = null;
      Walk(a);
      var ret = LastExpr;
      LastExpr = keep;
      return ret;
   }

   public override void WalkCall(Symbol function, ExprArgWalker<Any, Core.Expr> args) {
      
   }
   public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Core.Expr> args) {
      
   }

   public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
      
   }

   public override void WalkModule(bool isBare, Symbol name, Core.Expr block) => throw new NotSupportedException();
   public override void WalkStruct(StructKind kind, Symbol name, Symbol extends, Core.Expr block) => throw new NotSupportedException();
   public override void WalkAbstractType(Symbol name, Symbol extends, BuiltinType type) => throw new NotSupportedException();
   public override void WalkPrimitiveType(Symbol name, int bits, Symbol extends) => throw new NotSupportedException();

   public override void WalkBlock(Symbol head, ExprArgWalker<Any, Core.Expr> items) {
      
   }

   public override void WalkTuple(ExprArgWalker<Any, Core.Expr> items) {
      
   }

   public override void WalkLineNumberNode(int line, string file) {
      
   }

   public override Symbol ToSymbol(Any a) => (Symbol) a;
   public override long ToInteger(Any a) => Spinor.UnboxInt64(a);
   public override double ToFloat(Any a) => Spinor.UnboxFloat64(a);
   public override void WalkSymbol(Symbol s) => WalkLiteral(Spinor.Box(s), ExprType.Symbol);
   public override void WalkBool(bool b) => WalkLiteral(Spinor.Box(b), ExprType.Bool);
   public override void WalkInteger(long l) => WalkLiteral(Spinor.Box(l), ExprType.Integer);
   public override void WalkFloat(double d)  => WalkLiteral(Spinor.Box(d), ExprType.Float);
   public override void WalkLiteral(Any a, ExprType ty) => LastExpr = new Literal(a, ty);
}

