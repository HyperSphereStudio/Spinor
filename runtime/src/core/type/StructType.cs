/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using System.Linq;
using System.Reflection;
using Core;
using runtime.ILCompiler;
using Module = Core.Module;

namespace runtime.core.type;
using static TypeAttributes;

public enum StructKind : byte{
   Mutable,
   Immutable,
   Struct,
   Class
}

public record struct Field(SType Type, Symbol Name){
   public FieldInfo FieldInfo { get; internal set; }
}

public sealed class StructType : SType {
   public readonly StructKind Kind;
   public readonly Field[] Fields;
   public ConstructorInfo Constructor { get; private set; }

   internal StructType(StructKind kind, Symbol name, AbstractType super, Module module, Type underlyingType,
      Field[] fields) : base(name, super, module,
      underlyingType, true) {
         Fields = fields;
         Kind = kind;
   }
   
   public static StructType Create(StructKind kind, Symbol name, AbstractType super, RuntimeModule module, params Field[] fields) {
      super ??= Any.RuntimeType;
      var parent = kind == StructKind.Struct ? typeof(ValueType) : typeof(object);
      var ta = Public | Sealed;

      if (kind == StructKind.Struct)
         ta |= SequentialLayout | BeforeFieldInit;
      
      var tb = module.ModuleBuilder.DefineType(module.GetFullName(name), ta, parent);   
      
      tb.CreateInterfaceImplementation(super.UnderlyingType);
      
      var cb = tb.CreateConstructor(MethodAttributes.Public, fields.Select(x => x.Type.UnderlyingType).ToArray());
      
      IlExprBuilder eb = new(cb);
      
      for (var i = 0; i < fields.Length; i++) {
         var fb = tb.DefineField(fields[i].Name.String, fields[i].Type.UnderlyingType, FieldAttributes.Public | FieldAttributes.InitOnly);
         eb.Load.This(false);
         eb.Load.Arg(i);
         eb.Store.Field(fb);
      }
      
      eb.ReturnVoid();

      StructType st = new(kind, name, super, module, tb, fields);

      st.Initialize();
      
      return st;
   }

   protected override Type Initialize() {
      var type = base.Initialize();
      Constructor = type.GetConstructors()[0];
      
      var fields = type.GetFields();
      for (int i = 0; i < fields.Length; i++)
         Fields[i].FieldInfo = fields[i];
      
      return type;
   }
}