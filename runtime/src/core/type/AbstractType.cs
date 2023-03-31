/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using runtime.core.internals;
using runtime.ILCompiler;
using runtime.stdlib;
using Module = runtime.stdlib.Module;

namespace runtime.core.type;

using static TypeAttributes;

public sealed class AbstractType : SType {
   public AbstractType(Symbol name, SType super, Module module, int specificity, Type type) : 
      base(name, super, module, 0, specificity, 
         new TypeLayout(0, type, null, null, null)) {}
}

public class AbstractTypeBuilder : STypeBuilder{
   public int Specificity => Super.Specificity + 1;
   
   public AbstractTypeBuilder() {}
   
   public override AbstractType Create() {
      var ty = Spinor.Root.ModuleScope.DefineType(Module.GetFullName(Name), Interface | Public | Abstract);
      ty.AddInterfaceImplementation(typeof(IAny));
      return (AbstractType) base.Create(ty);
   }

   protected override SType InitializeType(Type underlyingType) => new AbstractType(Name, Super, Module, Specificity, underlyingType);
   protected override void WriteInitialization(Type underlyingType, IlExprBuilder ti, FieldInfo moduleField) {
       ti.Load
          .Field(moduleField)
          .String(Name.String);
       ti.Function.Invoke(Super.GetRuntimeTypeMethod);
       ti.Load
          .Int32(Specificity)
          .Type(underlyingType);
       ti.Function.Invoke(Reflect.Module_InitializeAbstractType, true);
   }
}