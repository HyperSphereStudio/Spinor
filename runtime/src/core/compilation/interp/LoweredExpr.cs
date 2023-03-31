/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using Core;
using runtime.core.internals;
using runtime.core.math;
using runtime.core.parse;
using runtime.core.type;
using runtime.stdlib;

namespace runtime.core.compilation.interp;

public interface ILoweredExpr
{
   public virtual SType Type => Any.RuntimeType;
}

public record Block() : ILoweredExpr;
public record Literal(Any Value, ExprType Type) : ILoweredExpr;

internal class ExprLowerer : AbstractSpinorExprWalker {
   public ILoweredExpr LastExpr;
   private readonly CompileTimeModule _Module;
   
   internal ExprLowerer(CompileTimeModule module) {
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

   public override void WalkCall(Expr function, ExprArgWalker<Any, Expr> args) {
      
   }
   public override void WalkOperatorCall(SpinorOperator op, ExprArgWalker<Any, stdlib.Expr> args) {
      
   }

   public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
      
   }

   public override void WalkModule(bool isBare, Symbol name, Expr block) => throw new NotSupportedException();
   public override void WalkStruct(SpinorTypeAttributes kind, Symbol name, Any extends, Expr block) => throw new NotSupportedException();
   public override void WalkAbstractType(Symbol name, Any extends) => throw new NotSupportedException();
   public override void WalkUsing(bool isSystem, ExprArgWalker<Any, Expr> n) {
      
   }

   public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {
      
   }

   public override void WalkTuple(ExprArgWalker<Any, stdlib.Expr> items) {
      
   }

   public override void WalkLineNumberNode(int line, string file) {
      
   }

   public override Symbol ToSymbol(Any a) => a;
   public override long ToInteger(Any a) => (long) a;
   public override double ToFloat(Any a) => (double)a;
   public override void WalkName(Symbol s){}

   public override void WalkSymbol(Symbol s) => WalkLiteral(s, ExprType.Symbol);
   public override void WalkNothing() {}

   public override void WalkBool(bool b) => WalkLiteral(b, ExprType.Bool);
   public override void WalkInteger(long l) => WalkLiteral(l, ExprType.Integer);
   public override void WalkFloat(double d)  => WalkLiteral(d, ExprType.Float);
   public override void WalkLiteral(Any a, ExprType ty) => LastExpr = new Literal(a, ty);
}

