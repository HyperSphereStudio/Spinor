/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using System.Reflection.Emit;
using Core;
using runtime.core.memory;
using runtime.ILCompiler;
using Module = Core.Module;

namespace runtime.core.type;

using static MethodAttributes;


public sealed class NumericType : PrimitiveType {
    public FieldInfo ValueField { get; private set; }

    private NumericType(Symbol name, AbstractType super, Module module, Type underlyingType, FieldInfo valueField, int bytecount) :
        base(name, super, module, underlyingType, bytecount) => ValueField = valueField;

    internal static NumericType _Create(Symbol name, AbstractType super, TypeBuilder tb, CompileTimeModule module, int bytelength) {
        Type numericType = super.Builtin switch {
            BuiltinType.FloatingNumber =>
                bytelength switch {
                    2 => typeof(Half),
                    4 => typeof(float),
                    8 => typeof(double),
                    16 => typeof(Decimal),
                    _ => null
                },
            
            BuiltinType.SignedInteger => 
                bytelength switch {
                    1 => typeof(sbyte),
                    2 => typeof(Int16),
                    4 => typeof(Int32),
                    8 => typeof(Int64),
                    16 => typeof(Int128),
                    _ => null
                },
            
            BuiltinType.UnsignedInteger => 
                bytelength switch {
                    1 => typeof(byte),
                    2 => typeof(UInt16),
                    4 => typeof(UInt32),
                    8 => typeof(UInt64),
                    16 => typeof(UInt128),
                    _ => null
                },
            
            BuiltinType.None => throw new SpinorException("Not a Builtin Type!")
        };

        if (numericType == null)
            throw new SpinorException($"Unknown Numeric Type! {super.Builtin}::{bytelength}");
        
        return _Create(name, super, numericType, tb, module, bytelength);
    }

    private static NumericType _Create(Symbol name, AbstractType super, Type numericType, TypeBuilder tb, CompileTimeModule module, int byteLength) {
        var fb = tb.DefineField("Value", numericType, FieldAttributes.Public | FieldAttributes.InitOnly);
        var cb  = tb.CreateConstructor(Public, numericType);
        
        cb.DefineParameter(1, ParameterAttributes.None, "value");
        IlExprBuilder eb = new(cb);
        eb.Load.This(false);
        eb.Load.Arg(0);
        eb.Store.Field(fb);
        eb.ReturnVoid();
        
        IlExprBuilder mb = new(tb.CreateMethod("ToString", typeof(string), Public | Virtual));
        tb.Override(mb, Reflect.StructToStringMI);
        mb.Load.This(false);
        mb.Load.FieldAddr(fb);
        mb.Function.Invoke(numericType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, System.Type.EmptyTypes), true);
        mb.Return();

        mb = new(tb.DefineMethod("Serialize", 
            Private | Virtual | Final | HideBySig, typeof(void), new []{typeof(SpinorSerializer)}));
        mb.DefineArg(0, "x");
        mb.Load.Arg(0);
        mb.Load.This();
        mb.Load.FieldValue(fb);
        mb.Function.Invoke(Reflect.SerializedWriteMI.MakeGenericMethod(numericType));
        tb.Override(mb, Reflect.SerializeMI);
        mb.ReturnVoid();

        return new(name, super, module, tb, fb, byteLength);
    }

    protected override Type Initialize() {
        var ty = base.Initialize();
        ValueField = ty.GetField("Value");

        return ty;
    }
}