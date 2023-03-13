/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System.Collections.Generic;
using Core;
using runtime.core.Compilation;
using runtime.core.parse;
using runtime.core.type;
using runtime.stdlib;
using runtime.Utils;

namespace runtime.core.compilation.Expr;

public class GlobalExprInterpreter : AbstractSpinorExprWalker {
   public readonly CompileTimeModule Module;
   
   private readonly ExprLowerer Lowerer;
   private readonly Dictionary<Symbol, Any> Locals = new();
   private Any _lastValue;
   private int Line = 0;
   private string File = "";

   public GlobalExprInterpreter(CompileTimeModule module) {
      Module = module;
      Lowerer = new(module);
   }

   public Any Evaluate(Any a) {
      Walk(a);
      return _lastValue;
   }
   
   public T GetName<T>(Symbol s) {
      if (s == null)
         return default;
      if (Locals.TryGetValue(s, out var name))
         return (T) name;
      if (Module.HasName(s))
         return (T) Module[s];
      throw new SpinorException($"Unable to Find Name{s}");
   }

   public override void ThrowParsingError(string msg, string help = null) => throw new SpinorException($"{0}\t{Line}:{File}");
   
   public override void WalkCall(Symbol function, ExprArgWalker<Any, Core.Expr> args) {
      
   }

   public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Core.Expr> args) {
      
   }

   public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
      
   }

   public override void WalkModule(bool isBare, Symbol name, Core.Expr block) => _lastValue = Module.NewModule(name, isBare);
   public override void WalkAbstractType(Symbol name, Symbol extends, BuiltinType builtinType) => _lastValue = Module.NewAbstractType(name, GetName<AbstractType>(extends), builtinType);
   public override void WalkPrimitiveType(Symbol name, int bits, Symbol extends) => _lastValue = Module.NewPrimitiveType(name,GetName<AbstractType>(extends), ByteMath.PromoteBytes(ByteMath.Bits2Bytes(bits)));

   public override void WalkStruct(StructKind kind, Symbol name, Symbol extends, Core.Expr block) {
      List<Field> fields = new();

      foreach (var item in block.Args) {
         if (item is LineNumberNode l) {
            WalkLineNumberNode(l.Line, l.File);
            continue;
         }
         
         ParseName(item, out var fieldName, out var fieldType);
         var fty = fieldType != null ? GetName<SType>(fieldType) : Any.RuntimeType;
         fields.Add(new(fty, fieldName));
      }
      
      var super = extends == null ? GetName<AbstractType>(extends) : null;
      _lastValue = Module.NewStructType(kind, name, super, fields.ToArray());
   }

   public override void WalkBlock(Symbol head, ExprArgWalker<Any, Core.Expr> items) {
       foreach(var item in items)
          Walk(item);
   }

   public override void WalkTuple(ExprArgWalker<Any, Core.Expr> items) {
        
   }

   public override void WalkLineNumberNode(int line, string file) {
      Line = line;
      File = file;
   }
   
   public override void WalkLiteral(Any a, ExprType type) => _lastValue = a;
   public override void WalkSymbol(Symbol s) => WalkLiteral(Spinor.Box(s), ExprType.Symbol);
   public override void WalkBool(bool b) => WalkLiteral(Spinor.Box(b), ExprType.Bool);
   public override void WalkInteger(long l) => WalkLiteral(Spinor.Box(l), ExprType.Integer);
   public override void WalkFloat(double d)  => WalkLiteral(Spinor.Box(d), ExprType.Float);
}