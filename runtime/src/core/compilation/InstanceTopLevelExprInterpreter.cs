/*
   * Author : Johnathan Bizzano
   * Created : Sunday, January 22, 2023
   * Last Modified : Sunday, January 22, 2023
*/

using System;
using System.Reflection;
using System.Reflection.Emit;
using Core;
using runtime.core.Compilation;
using runtime.ILCompiler;
using runtime.stdlib;
using Module = Core.Module;

namespace runtime.core.compilation;



public class InstanceTopLevelExprInterpreter<TAny, TExpr> : ISpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny> {
   private readonly InstanceExprCompiler<TAny, TExpr> _exprCompiler;

   public InstanceTopLevelExprInterpreter(ISpinorExprWalker<TAny, TExpr> reader) => _exprCompiler = new(reader); 

   private ISpinorExprWalker<TAny, TExpr> This => this;

   protected void Interpret(TAny a, Module topModule) {
      
   }
   
   #region Reading

   public void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args) {
    
   }

   public void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args) {
     
   }

   public void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs) {
      
   }

   public void WalkModule(bool isBare, Symbol name, TExpr block) {
      
   }

   public void WalkStruct(bool isMutable, Symbol name, TExpr block) {
      
   }

   public void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items) {
    
   }

   public void WalkTuple(ExprArgWalker<TAny, TExpr> items) {
     
   }

   public void WalkLineNumberNode(int line, string file) {
      
   }

   public void WalkAbstractType(Symbol name, Symbol extends, bool isBuiltin) {
     
   }

   public void WalkPrimitiveType(Symbol name, int bits, Symbol extends) {
     
   }

   public TExpr ToExpr(TAny a) => _exprCompiler.ToExpr(a);
   public bool ToBool(TAny a) => _exprCompiler.ToBool(a);
   public void ToLineNumberNode(TAny a, out int line, out string file) => _exprCompiler.ToLineNumberNode(a, out line, out file);
   public Symbol ToSymbol(TAny a) => _exprCompiler.ToSymbol(a);
   public long ToInteger(TAny a) => _exprCompiler.ToInteger(a);
   public double ToFloat(TAny a) => _exprCompiler.ToFloat(a);
   public void WalkSymbol(Symbol s){}

   public void WalkBool(bool b){}
   public void WalkInteger(long l){}
   public void WalkFloat(double d){}

   public ExprType GetType(TAny a) => _exprCompiler.GetType(a);
   #endregion
}