/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using Core;
using Module = Core.Module;

namespace runtime.core.type;

using static TypeAttributes;

public enum BuiltinType : byte {
   None,
   
   SignedInteger,
   UnsignedInteger,
   FloatingNumber,
   Exception,
   
   NumericStart = SignedInteger,
   NumericEnd = FloatingNumber
}

public sealed class AbstractType : SType {
   public readonly BuiltinType Builtin;

   internal AbstractType(Symbol name, AbstractType super, Module module, Type underlyingType, BuiltinType builtin) : base(name, super, module, underlyingType, false) => Builtin = builtin;
   
   internal static AbstractType Create(Symbol name, AbstractType super, RuntimeModule module, BuiltinType builtinType = BuiltinType.None) {
      super ??= Any.RuntimeType;
      var ty = module.ModuleBuilder.DefineType(module.GetFullName(name), Interface | Public | Abstract);
      ty.AddInterfaceImplementation(super.UnderlyingType);
      var aty = new AbstractType(name, super, module, ty, builtinType);
      aty.Initialize();
      return aty;
   }
}