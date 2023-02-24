/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using Core;
using runtime.ILCompiler;
using Module = Core.Module;

namespace runtime.core.type;


public enum BuiltinType : byte {
   None,
   SignedInteger,
   UnsignedInteger,
   FloatingNumber
}

public sealed class AbstractType : SType {
   public const TypeAttributes AbstractAttributes = TypeAttributes.Interface | TypeAttributes.Public | TypeAttributes.Abstract;

   public readonly BuiltinType Builtin;

   internal AbstractType(Symbol name, AbstractType super, Module module, Type underlyingType, BuiltinType builtin) : base(name, super, module, underlyingType) => Builtin = builtin;

   internal static AbstractType Create(Symbol name, AbstractType super, RuntimeModule module, BuiltinType builtinType = BuiltinType.None) => Create(name, super, (super ?? Any.RuntimeType).UnderlyingType, module, builtinType);

   internal static AbstractType Create(Symbol name, AbstractType ssuper, Type super, RuntimeModule module, BuiltinType builtinType = BuiltinType.None) {
      var ty = module.ModuleBuilder.DefineType(module.GetFullName(name), AbstractAttributes);
      ty.AddInterfaceImplementation(super);
      ty.CreateBackingGetSetProperty("RuntimeType", typeof(SType), Attributes.Static | Attributes.Public);
      return new(name, ssuper, module, ty, builtinType);
   }
   
   internal static AbstractType CreateBuiltin(Symbol name, AbstractType super, RuntimeModule module) {
      BuiltinType builtinType;
      
      if (name == CommonSymbols.Signed)
         builtinType = BuiltinType.SignedInteger;
      else if (name == CommonSymbols.Unsigned)
         builtinType = BuiltinType.UnsignedInteger;
      else if (name == CommonSymbols.AbstractFloat)
         builtinType = BuiltinType.FloatingNumber;
      else throw new SpinorException("Unknown Builtin Type {0}", name);
      
      return Create(name, super, module, builtinType);
   }
}