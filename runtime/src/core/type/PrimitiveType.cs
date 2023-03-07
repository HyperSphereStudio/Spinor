/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using runtime.core;
using runtime.core.memory;
using runtime.core.type;
using runtime.ILCompiler;

namespace Core;

using static TypeAttributes;

public interface IPrimitiveValue : IAny{
    void Any.Serialize(SpinorSerializer serializer){}
}

public class PrimitiveType : SType
{
    public ushort ByteCount { get; }
    public ConstructorInfo Constructor { get; private set; }

    public override bool IsReference => false;
    public override bool IsMutable => false;
    public override bool IsSystemWrapper => true;
    public override bool IsUnmanagedType => true;
    public override bool IsConcrete => true;
    public override int StackLength => ByteCount;

    public PrimitiveType(Symbol name, AbstractType super, Module module, Type underlyingType, int bytecount) : base(
        name, super, module, underlyingType, true) {
        ByteCount = (ushort) bytecount;
    }

    internal static PrimitiveType Create(Symbol name, AbstractType super, RuntimeModule module, int bytelength) {
        PrimitiveType pty = null;
        super ??= Any.RuntimeType;
        
        var tb = module.ModuleBuilder.DefineType(module.GetFullName(name), 
                        Public | Sealed | SequentialLayout | BeforeFieldInit, 
                        typeof(ValueType), bytelength);

        if (super.Builtin != BuiltinType.None) {
            if (super.Builtin is >= BuiltinType.NumericStart and <= BuiltinType.NumericEnd)
                pty = NumericType._Create(name, super, tb, module, bytelength);
            else
                throw new SpinorException($"Cannot Create Primitive Type From {super.Builtin}");
        }

        if(super != null)
          tb.CreateInterfaceImplementation(super.UnderlyingType);
        
        tb.CreateInterfaceImplementation(typeof(IPrimitiveValue));

        pty ??= new(name, super, module, tb, bytelength);
        pty.Initialize();
        
        return pty;
    }

    protected override Type Initialize() {
        var ty = base.Initialize();
        Constructor = ty.GetConstructors()[0];
        return ty;
    }
}