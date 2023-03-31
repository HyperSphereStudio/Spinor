/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core;
using runtime.core.compilation.ILCompiler;
using runtime.core.function;
using runtime.core.internals;
using runtime.core.type;
using runtime.ILCompiler;
using runtime.Utils;

namespace runtime.stdlib;

using static SpinorTypeAttributes;
using static MethodAttributes;

[Flags]
public enum SpinorTypeAttributes : byte{
    None = 0,
    Mutable = 1,
    Unmanaged = 1<<2,
    Concrete = 1<<3,
    Class = 1<<4 | Concrete
}

[Flags]
public enum SpinorFieldAttributes : byte {
    None,
    Global,
    Unmanaged
}

public readonly record struct DynamicSpinorField(Symbol Name, FieldBuilder Field, SType FieldType, SpinorFieldAttributes Attributes);
public readonly record struct SpinorField(Symbol Name, RuntimeFieldHandle FieldHandle, SType FieldType, SpinorFieldAttributes Attributes) {
    public SpinorField(FieldInfo f) : this((Symbol) f.Name, f.FieldHandle, f.FieldType.SType(), GetFieldAttributes(f)) {}
    public SpinorField(string name, RuntimeFieldHandle fieldHandle, SType fieldType, SpinorFieldAttributes attributes) : 
        this((Symbol) name, fieldHandle, fieldType, attributes) {}
    
    public FieldInfo Field => FieldInfo.GetFieldFromHandle(FieldHandle);
    public static SpinorFieldAttributes GetFieldAttributes(FieldInfo f) {
        SpinorFieldAttributes a = 0;
        if (f.IsStatic)
            a |= SpinorFieldAttributes.Global;
        if (f.FieldType.SType().IsUnmanaged)
            a |= SpinorFieldAttributes.Unmanaged;
        return a;
    }
    public Any GetValue(Any instance) => Field.GetValue(instance.Value).BoxObject();
    public void SetValue(Any instance, Any value) => Field.SetValue(instance.Value, Method.UnboxObjectForMethodCall(FieldType, value));
}
public readonly record struct DynamicSpinorProperty(Symbol Name, PropertyBuilder Property, SType FieldType, SpinorFieldAttributes Attributes);
public readonly record struct SpinorProperty(Symbol Name, RuntimeMethodHandle GetMethodHandle, RuntimeMethodHandle SetMethodHandle, SpinorFieldAttributes Attributes, SType FieldType) {
    public SpinorProperty(PropertyInfo p) : 
        this((Symbol) p.Name, p.GetMethod?.MethodHandle ?? default, 
        p.SetMethod?.MethodHandle ?? default, GetPropertyAttributes(p, out var ft), ft) {}
    public SpinorProperty(string name, RuntimeMethodHandle getMethodHandle, RuntimeMethodHandle setMethodHandle, SType fieldType, SpinorFieldAttributes attributes) : 
        this((Symbol) name, getMethodHandle, setMethodHandle, attributes, fieldType) {}
    public MethodInfo GetProperty => (MethodInfo) MethodInfo.GetMethodFromHandle(GetMethodHandle);
    public MethodInfo SetProperty => (MethodInfo) MethodInfo.GetMethodFromHandle(SetMethodHandle);
    public static SpinorFieldAttributes GetPropertyAttributes(PropertyInfo p, out SType fieldType) {
        SpinorFieldAttributes a = 0;
        var method = p.GetMethod ?? p.SetMethod;
        fieldType = (p.GetMethod?.ReturnType ?? p.SetMethod.GetParameters()[0].ParameterType).SType();
        if (method.IsStatic)
            a |= SpinorFieldAttributes.Global;
        if (fieldType.IsUnmanaged)
            a |= SpinorFieldAttributes.Unmanaged;
        return a;
    }
    
    public Any GetValue(Any instance) => GetProperty.Invoke(instance.Value, null).BoxObject();
    public void SetValue(Any instance, Any value) => SetProperty.Invoke(instance.Value, new[]{Method.UnboxObjectForMethodCall(FieldType, value)});
}
public readonly record struct SpinorParameter(Symbol Name, SType Extends);

public readonly record struct TypeLayout(ushort DataLength, RuntimeTypeHandle TypeHandle, SpinorField[] Fields, SpinorProperty[] Properties, SpinorParameter[] Parameters){
    public TypeLayout(int dataLength, Type t, SpinorField[] fields, SpinorProperty[] properties, SpinorParameter[] parameters) : 
        this((ushort) dataLength, t.TypeHandle, 
            fields ?? Array.Empty<SpinorField>(), 
            properties ?? Array.Empty<SpinorProperty>(), 
            parameters ?? Array.Empty<SpinorParameter>()){}

    public Type Type => Type.GetTypeFromHandle(TypeHandle);
}

public abstract class SType : ISpinorAny {
    static SType() {
        var name = (Symbol) "Type";
        
        if (Spinor.Core.TryGetName(name, out var t)) {
            RuntimeType = t;
        }else{
            RuntimeType = new StructType(name, Any.RuntimeType, 
                Spinor.Root, Class, 1, 
                null,
                new TypeLayout(0, typeof(SType), new SpinorField[8], null, null));
            Spinor.Root.SetConst(name, RuntimeType);
        }
    }
    
    public readonly Symbol Name;
    public SType Super { get; internal set; }
    public readonly Module Module;
    public readonly SpinorTypeAttributes Attributes;
    public readonly ushort Specificity;
    public readonly TypeLayout Layout;
    
    public SType(Symbol name, SType super, Module module, SpinorTypeAttributes attributes, int specificity, TypeLayout layout) {
        Name = name;
        Super = super;
        Module = module;
        Attributes = attributes;
        Specificity = (ushort) specificity;
        Layout = layout;
        Spinor.Types.Add(layout.Type, this);
    }

    public static SType RuntimeType { get; }
    public SType Type => RuntimeType;
    public bool IsMutable => Attributes.HasFlag(Mutable);
    public bool IsClass => Attributes.HasFlag(Class);
    public bool IsUnmanaged => Attributes.HasFlag(Unmanaged);
    public bool IsConcrete => Attributes.HasFlag(Concrete);
    public unsafe int StackLength => IsClass ? sizeof(IntPtr) : Layout.DataLength;
    
    public bool IsAssignableTo(SType t) => UnderlyingType.IsAssignableTo(t.UnderlyingType);
    public bool IsAssignableFrom(SType t) => UnderlyingType.IsAssignableFrom(t.UnderlyingType);
    void IAny.Print(TextWriter tw) => tw.Write(Name.String);
    public override string ToString() => Name.String;
    public static implicit operator Any(SType s) => new(s);
    public static implicit operator SType(Any a) => a.Cast<SType>();
    public virtual MethodInfo GetRuntimeTypeMethod => Type == Any.RuntimeType ? Reflect.Any_GetRuntimeType : Layout.Type.GetMethod("get_RuntimeType");
    public Type UnderlyingType => Layout.Type;
}

public abstract class STypeBuilder {
    public Symbol Name;
    public AbstractType Super;
    public CompileTimeModule Module;
    public SpinorTypeAttributes Attributes;

    public bool IsMutable {
        get => Attributes.HasFlag(Mutable);
        set => Attributes = Attributes.Set(Mutable, value);
    }
    public bool IsClass {
        get => Attributes.HasFlag(Class);
        set => Attributes = Attributes.Set(Class, value);
    }
    public bool IsUnmanaged {
        get => Attributes.HasFlag(Unmanaged);
        set => Attributes = Attributes.Set(Unmanaged, value);
    }
    public bool IsConcrete => Attributes.HasFlag(Concrete);

    public abstract SType Create();
    
    protected SType Create(TypeBuilder tb) {
        Super ??= (AbstractType) Any.RuntimeType;
        
        tb.AddInterfaceImplementation(Super.UnderlyingType);
        
        var runtimeTypeField = WriteRuntimeTypeHandle(tb);

        if (IsConcrete)
            WriteGetTypeImpl(tb, runtimeTypeField);
        
        WriteTypeInit(tb, runtimeTypeField);
        
        try {
            var ty = InitializeType(tb.CreateType());
            Module.SetConst(Name, ty);
            return ty;
        }catch (Exception) {
            "Error while Creating Type {0}".PrintLn(Name);
            throw;
        }
    }

    protected abstract SType InitializeType(Type underlyingType);
    protected abstract void WriteInitialization(Type underlyingType, IlExprBuilder typeInitializer, FieldInfo moduleField);


    private void WriteTypeInit(TypeBuilder tb, FieldInfo runtimeTypeField) {
        IlExprBuilder rtb = tb.DefineTypeInitializer();
        WriteInitialization(tb, rtb, Module.ModuleField);
        rtb.Store.Field(runtimeTypeField);
        rtb.ReturnVoid();
    }

    private static void WriteGetTypeImpl(TypeBuilder tb, FieldInfo runtimeTypeField) {
        IlExprBuilder sMb = new(tb.CreateMethod("get_Type", typeof(SType), Public|Virtual|Final|SpecialName|HideBySig|NewSlot));
        sMb.Load.Field(runtimeTypeField);
        sMb.Return();
        tb.CreateProperty("Type", typeof(SType)).SetGetMethod(sMb);
    }

    private FieldBuilder WriteRuntimeTypeHandle(TypeBuilder tb) {
        var runtimeTypeField = tb.CreateField("__RuntimeType__", typeof(SType), FieldAttributes.Private|FieldAttributes.Static|FieldAttributes.InitOnly);
        var sMb = tb.ImplementBackedGetMethod("RuntimeType", runtimeTypeField, Public|Static);
        tb.Override(sMb, Reflect.IAny_GetRuntimeType);
        tb.CreateProperty("RuntimeType", typeof(SType)).SetGetMethod(sMb);
        return runtimeTypeField;
    }
}