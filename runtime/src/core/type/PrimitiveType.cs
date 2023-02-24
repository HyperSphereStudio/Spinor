/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using runtime.core;
using runtime.core.type;
using runtime.ILCompiler;

namespace Core;

public interface IPrimitiveValue<T> : IAny<T> where T: unmanaged, IAny {}

public class PrimitiveType : SType {
   
   public const TypeAttributes PrimitiveAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.BeforeFieldInit;
  
   public ushort ByteCount { get; }
   public ConstructorInfo Constructor { get; private set; }
   
   public override bool IsReference => false;
   public override bool IsMutable => false;
   public override bool IsSystemWrapper => true;
   public override bool IsUnmanagedType => true;
   public override int StackLength => ByteCount;

   public PrimitiveType(Symbol name, AbstractType super, Module module, Type underlyingType, int bytecount) : base(name, super, module, underlyingType) {
      ByteCount = (ushort) bytecount;
   }
   
   internal static PrimitiveType Create(Symbol name, AbstractType super, RuntimeModule module, int bytelength) {
      PrimitiveType pty = null;
      //Create Wrapper Type
      ILTypeBuilder tb = new(module.ModuleBuilder.DefineType(module.GetFullName(name), PrimitiveAttributes, typeof(ValueType), bytelength), false);
      
      if (super != null && super.Builtin != BuiltinType.None) {
         pty = NumericType._Create(name, super, tb, module, bytelength);
      }
      
      //<: Super
      //if(super != null)
        // tb.AddInterface(super.UnderlyingType);
      var at = Attributes.Static | Attributes.Public;
      tb.CreateBackingGetSetProperty("RuntimeType", typeof(SType), at, at, at, at | Attributes.Delete);
      tb.AddInterface(Reflect.IPrimitiveValue_TY.MakeGenericType(tb));
      var mb = tb.CreateMethod("get_This", tb, MethodAttributes.Public | IlExprBuilder.InterfaceAttributes);
      mb.Load.This();
      mb.Return();

      return pty ?? new(name, super ?? Any.RuntimeType, module, tb, bytelength);
   }

   public override Type Initialize() {
      var ty = base.Initialize();
      Constructor = ty.GetConstructors()[0];
      return ty;
   }
}
