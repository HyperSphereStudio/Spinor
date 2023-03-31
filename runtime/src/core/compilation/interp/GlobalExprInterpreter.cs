/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using System.Collections.Generic;
using System.Text;
using runtime.core.internals;
using runtime.core.math;
using runtime.core.type;
using runtime.stdlib;

namespace runtime.core.compilation.interp;

public sealed class GlobalExprInterpreter : AbstractSpinorExprWalker {
   public readonly CompileTimeModule Module;
   
   private readonly ExprLowerer Lowerer;
   private readonly Dictionary<Symbol, Any> Locals = new();
   private Any _lastValue;
   private int _line = 0;
   private string _file = "";

   public GlobalExprInterpreter(CompileTimeModule module) {
      Module = module;
      Lowerer = new(module);
   }

   public Any Evaluate(Any a) {
      _lastValue = Spinor.Nothing;
      Walk(a);
      return _lastValue;
   }
   
   public Any GetName(Symbol s){
      if (Locals.TryGetValue(s, out var name))
         return name;

      if (Module.TryGetName(s, out var v))
         return v;

      if (Module.TryGetSystemName(s, out var k))
         return k.SType();

      ThrowParsingError($"Unable to Find Name {s}");
      return default;
   }

   public override void ThrowParsingError(string msg, string help = null) => throw new SpinorException($"{msg}\t{_line}:{_file}");
   
   public override void WalkCall(Expr function, ExprArgWalker<Any, Expr> args) {
      if (function.Head == ASTSymbols.SysCall) {
          
      }
   }

   public override void WalkOperatorCall(SpinorOperator op, ExprArgWalker<Any, Expr> args) {
      
   }

   public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {
      
   }

   public override void WalkModule(bool isBare, Symbol name, Expr block) => _lastValue = Module.NewModule(name, isBare);
   public override void WalkAbstractType(Symbol name, Any extends) => _lastValue = Module.NewAbstractType(name, Evaluate(extends).Cast<AbstractType>()).Create();
   public override void WalkUsing(bool isSystem, ExprArgWalker<Any, Expr> n) {
      if (isSystem) {
         StringBuilder sb = new();
         for (var i = 0; i < n.Length; i++) {
            if (i != 0)
               sb.Append('.');
            sb.Append(n[i]);
         }
         sb.PrintLn();
         Module.UsingSystem(sb.ToString());
      }
      else {
         if (!stdlib.Module.FindRootModule(ToSymbol(n[0]), out var m)) {
            throw new NotImplementedException();
         }
         for (var i = 1; i < n.Length; i++)
            m = m[ToSymbol(n[i])];
         _lastValue = Module.Using(m);
      }
   }

   public override void WalkStruct(SpinorTypeAttributes kind, Symbol name, Any extends, Expr block) {
      var tb = Module.NewStructType(name, Evaluate(extends).Cast<AbstractType>(), kind);

      foreach (var item in block.Args) {
         switch (item.Value) {
            case LineNumberNode l:
               WalkLineNumberNode(l.Line, l.File);
               break;
            case Symbol s:
               tb.AddField(s, Any.RuntimeType);
               break;
            case Expr e:
               if (e.Head == ASTSymbols.ElementOf)
                  tb.AddField( e[0], Evaluate(e[1]));
               break;
         }
      }
      
      _lastValue = new(tb.Create());
   }

   public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {
       foreach(var item in items)
          Walk(item);
   }

   public override void WalkTuple(ExprArgWalker<Any, Expr> items) {
       
   }

   public override void WalkLineNumberNode(int line, string file) {
      _line = line;
      _file = file;
   }
   
   public override void WalkLiteral(Any a, ExprType type) => _lastValue = a;
   public override void WalkName(Symbol s) => _lastValue = GetName(s);
   public override void WalkSymbol(Symbol s) => WalkLiteral(s, ExprType.Symbol);
   public override void WalkNothing() => _lastValue = Spinor.Nothing;
   public override void WalkBool(bool b) => WalkLiteral(b, ExprType.Bool);
   public override void WalkInteger(long l) => WalkLiteral(l, ExprType.Integer);
   public override void WalkFloat(double d)  => WalkLiteral(d, ExprType.Float);
}