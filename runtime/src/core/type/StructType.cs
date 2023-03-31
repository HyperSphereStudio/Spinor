/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core.compilation.ILCompiler;
using runtime.core.internals;
using runtime.ILCompiler;
using runtime.stdlib;
using Module = runtime.stdlib.Module;

namespace runtime.core.type;
using static TypeAttributes;
using static SpinorTypeAttributes;

public sealed class StructType : SType {
   public readonly RuntimeMethodHandle[] Constructors;

   public StructType(Symbol name, SType super, Module module, SpinorTypeAttributes attributes,
      int specificity, RuntimeMethodHandle[] constructors, TypeLayout layout) :
      base(name, super, module, attributes, specificity, layout) { Constructors = constructors ?? Array.Empty<RuntimeMethodHandle>(); }
}

public class StructTypeBuilder : STypeBuilder {
   private int _typeSize;
   public readonly List<DynamicSpinorField> Fields = new();
   public readonly List<ConstructorInfo> Constructors = new();
   private ConstructorBuilder _ctor;
   public int Specificity => Super.Specificity + 1;
   
   public StructTypeBuilder() {}

   public void AddField(Symbol name, SType fieldType = null, SpinorFieldAttributes attributes=0) {
       Fields.Add(new(){Name=name, Field=default, FieldType=fieldType, Attributes=attributes});
   }

   public override StructType Create() {
      Attributes |= Concrete;
      
      var parent = IsClass ? typeof(object) : typeof(ValueType);
      var ta = Public | Sealed;

      if (!IsClass)
         ta |= SequentialLayout | BeforeFieldInit;
      
      var tb = Spinor.Root.ModuleScope.DefineType(Module.GetFullName(Name), ta, parent);
      
      tb.AddInterfaceImplementation(typeof(ISpinorAny));
      
      _ctor = tb.CreateConstructor(MethodAttributes.Private, Fields.Select(x => GetFieldType(x.FieldType)).ToArray());

      IlExprBuilder eb = new(_ctor);
      _typeSize = 0;
      var isUnmanaged = !IsClass;
      
      for (var i = 0; i < Fields.Count; i++) {
         var f = Fields[i];
         _typeSize += f.FieldType.Layout.DataLength;

         var fa = FieldAttributes.Public;
         
         if (!IsMutable)
            fa |= FieldAttributes.InitOnly;
         
         if (f.Attributes.HasFlag(SpinorFieldAttributes.Global))
            fa |= FieldAttributes.Static;
         
         var fb = tb.DefineField(f.Name.String, GetFieldType(f.FieldType), fa);
         
         eb.Load.This(false);
         eb.Load.Arg(i);
         eb.Store.Field(fb);
         Fields[i] = Fields[i] with { Field = fb, FieldType = Fields[i].FieldType ?? Any.RuntimeType};
         isUnmanaged &= f.FieldType.IsUnmanaged;
      }
      eb.ReturnVoid();
     
      if (isUnmanaged)
         Attributes |= Unmanaged;
      
      return (StructType) base.Create(tb);
   }

   protected override SType InitializeType(Type underlyingType) {
      var tyFields = underlyingType.GetFields();
      var spFields = new SpinorField[tyFields.Length];
      var ctorHandles = underlyingType.GetConstructors().Select(x => x.MethodHandle).ToArray();
      
      for (var i = 0; i < spFields.Length; i++)
         spFields[i] = new(Fields[i].Name, tyFields[i].FieldHandle, Fields[i].FieldType, Fields[i].Attributes);
      
      return new StructType(Name, Super, Module, Attributes, Specificity, ctorHandles, 
         new TypeLayout(_typeSize, underlyingType, spFields, null, null));
   }

   private static Type GetFieldType(SType t) => (t == null || t == Any.RuntimeType) ? typeof(Any) : t.UnderlyingType;
   
   protected override void WriteInitialization(Type underlyingType, IlExprBuilder ti, FieldInfo moduleField) {
      var fieldArray = ti.Array.Serialize1D<DynamicSpinorField, SpinorField>(Fields, f => {
         ti.Load
            .String(f.Name.String)
            .Token(f.Field);
         ti.Function.Invoke(f.FieldType.GetRuntimeTypeMethod);
         ti.Load.Int32((int) f.Attributes);
         ti.Create.Object(Reflect.SpinorField_New);   
      });

      var ctorArray = ti.Array.Serialize1D<ConstructorInfo, RuntimeMethodHandle>(Constructors, c => ti.Load.Token(c));

       ti.Load
          .Field(moduleField)
          .Int32((int) Attributes)
          .String(Name.String);
       
       ti.Function.Invoke(Super.GetRuntimeTypeMethod);
       ti.Convert.CastClass(typeof(AbstractType));
       
       ti.Load
          .Int32(Specificity)
          .Local(ctorArray);
      
      ti.Load   
         .Int32(_typeSize)
         .Type(underlyingType)
         .Local(fieldArray)
         .Null() //Properties
         .Null(); //Parameters
      
       ti.Create.Object(Reflect.TypeLayout_New);
      
       ti.Function.Invoke(Reflect.Module_InitializeStructType, true);
   }
}