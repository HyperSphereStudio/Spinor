/*
   * Author : Johnathan Bizzano
   * Created : Sunday, February 19, 2023
   * Last Modified : Sunday, February 19, 2023
*/

using System;
using System.Reflection;
using System.Reflection.Emit;
using Core;
using runtime.ILCompiler;
using Module = Core.Module;

namespace runtime.core.type;


public sealed class NumericType : PrimitiveType {
    public FieldInfo ValueField { get; private set; }

    private NumericType(Symbol name, AbstractType super, Module module, Type underlyingType, FieldInfo valueField, int bytecount) :
        base(name, super, module, underlyingType, bytecount) => ValueField = valueField;

    internal static NumericType _Create(Symbol name, AbstractType super, TypeBuilder tb, RuntimeModule module, int bytelength) {
        Type numericType = super.Builtin switch {
            BuiltinType.FloatingNumber =>
                bytelength switch {
                    2 => typeof(Half),
                    4 => typeof(float),
                    8 => typeof(double),
                    _ => null
                },
            
            BuiltinType.SignedInteger => 
                bytelength switch {
                1 => typeof(sbyte),
                2 => typeof(short),
                4 => typeof(int),
                8 => typeof(long),
                _ => null
                },
            
            BuiltinType.UnsignedInteger => 
                bytelength switch {
                    1 => typeof(byte),
                    2 => typeof(ushort),
                    4 => typeof(uint),
                    8 => typeof(ulong),
                    _ => null
                },
            
            BuiltinType.None => throw new SpinorException("Not a Builtin Type!")
        };

        if (numericType == null)
            throw new SpinorException("Unknown Numeric Type! {0}::{1}", super.Builtin, bytelength);
        
        return _Create(name, super, numericType, tb, module, bytelength);
    }
    
    internal static NumericType _Create(Symbol name, AbstractType super, Type numericType, TypeBuilder tb, RuntimeModule module, int byteLength) {
        var fb = tb.DefineField("Value", numericType, FieldAttributes.Public | FieldAttributes.InitOnly);
        var cb  = tb.CreateConstructor(Attributes.Public, numericType);
        cb.DefineParameter(1, ParameterAttributes.None, "value");
        IlExprBuilder eb = new(cb);
        eb.Load.This(false);
        eb.Load.Arg(0);
        eb.Store.Field(fb);
        eb.ReturnVoid();
        return new(name, super, module, tb, fb, byteLength);
    }

    public override Type Initialize() {
        var ty = base.Initialize();
        ValueField = ty.GetField("Value");
        return ty;
    }
}