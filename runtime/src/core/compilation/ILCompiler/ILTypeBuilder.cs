using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core;
using runtime.Utils;

namespace runtime.ILCompiler;

public struct ILTypeBuilder {
    public readonly TypeBuilder InternalBuilder;
    public readonly IlExprBuilder TypeInitializer;
    public bool HasTypeInitializer => TypeInitializer.InternalMethod != null;

    public static implicit operator TypeBuilder(ILTypeBuilder tb) => tb.InternalBuilder;
    
    internal ILTypeBuilder(TypeBuilder t, bool createTypeInitializer = true) {
        if(createTypeInitializer)
            TypeInitializer = new IlExprBuilder(t.DefineTypeInitializer());
        
        InternalBuilder = t;
    }

    public FieldBuilder CreateField(string name, Type type, FieldAttributes attribs) => InternalBuilder.DefineField(name, type, attribs);
    public PropertyBuilder CreateProperty(string name, Type t, PropertyAttributes attribs = PropertyAttributes.None) => InternalBuilder.CreateProperty(name, t, attribs);
    public IlExprBuilder CreateMethod(string name, Type returnType, MethodAttributes attr, params Type[] parameters) => new(InternalBuilder.CreateMethod(name, returnType, attr, parameters));

    public void Override(MethodInfo mb, MethodInfo baseMethod) => InternalBuilder.Override(mb, baseMethod);
    public void AddInterface(Type t, params Type[] generic_types) => InternalBuilder.CreateInterfaceImplementation(t, generic_types);

    //  public IlExprBuilder CreateGetMethod(PropertyBuilder pb, Attributes attr, FieldBuilder basicGetField = null) => InternalBuilder.CreateGetMethod(pb, attr, basicGetField);
 //   public IlExprBuilder CreateSetMethod(PropertyBuilder pb, Attributes attr, FieldBuilder basicSetField = null) => InternalBuilder.CreateSetMethod(pb, attr, basicSetField);
    
    public IlExprBuilder CreateConstructor(MethodAttributes attr, params Type[] parameters) =>
        new(InternalBuilder.CreateConstructor(attr, parameters));

    public IlExprBuilder ImplementBackedSetMethod(string name, FieldBuilder fb, MethodAttributes attr) => InternalBuilder.ImplementBackedSetMethod(name, fb, attr);
    public IlExprBuilder ImplementBackedGetMethod(string name, FieldBuilder fb, MethodAttributes attr) => InternalBuilder.ImplementBackedGetMethod(name, fb, attr);
    

    public Type Create() {
        if (HasTypeInitializer) {
            TypeInitializer.ReturnVoid();
        }
        return InternalBuilder.CreateType();
    }

    public static bool IsAllowedName(string s) {
        if (!(char.IsLetter(s[0]) || s[0] == '_'))
            return false;
        foreach(var c in s)
            if (!(char.IsLetterOrDigit(c) || char.IsSeparator(c)))
                return false;
        return true;
    }
}

public static class BuilderExtensions {
    public const MethodAttributes
        RequiredMethodAttributes = 0,
        
        RequiredGetSetAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName,
        
        RequiredConstructorAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName;

    public const FieldAttributes 
        RequiredFieldAttributes = 0;
    
    public const PropertyAttributes 
        RequiredPropertyAttributes = 0;
    
    public static ConstructorBuilder CreateConstructor(this TypeBuilder tb, MethodAttributes attr,
        params Type[] parameters) =>
        tb.DefineConstructor(attr | RequiredConstructorAttributes, CallingConventions.Standard,
            parameters);

    public static IlExprBuilder ImplementBackedSetMethod(this TypeBuilder tb, string name, FieldBuilder fb, MethodAttributes attr) {
        IlExprBuilder eb = new(tb.CreateMethod("set_" + name, typeof(void), attr | RequiredGetSetAttributes, fb.FieldType));

        if(!fb.IsStatic)
            eb.Load.This(false);
        
        eb.Load.Arg(0);
        eb.Store.Field(fb);
        eb.ReturnVoid();

        return eb;
    }
    
    public static IlExprBuilder ImplementBackedGetMethod(this TypeBuilder tb, string name, FieldBuilder fb, MethodAttributes attr) {
        IlExprBuilder eb = new(tb.CreateMethod("get_" + name, fb.FieldType, attr | RequiredGetSetAttributes));

        if(!fb.IsStatic)
            eb.Load.This(false);
        
        eb.Load.FieldValue(fb);
        eb.Return();

        return eb;
    }

    public static MethodBuilder CreateMethod(this TypeBuilder tb, string name, Type returnType, MethodAttributes attr,
        params Type[] parameters) {
        var mb = tb.DefineMethod(name, attr | RequiredMethodAttributes, returnType, parameters);
        mb.SetImplementationFlags(MethodImplAttributes.Managed);
        return mb;
    }

    public static FieldBuilder CreateField(this TypeBuilder tb, string name, Type type, FieldAttributes attr) => 
        tb.DefineField(name, type, attr);
    public static PropertyBuilder CreateProperty(this TypeBuilder tb, string name, Type t, PropertyAttributes attribs = PropertyAttributes.None) => 
        tb.DefineProperty(name, attribs, t, Type.EmptyTypes);

    public static void Override(this TypeBuilder tb, MethodInfo mb, MethodInfo baseMethod) => tb.DefineMethodOverride(mb, baseMethod);

    public static void CreateInterfaceImplementation(this TypeBuilder tb, Type type, params Type[] generic_parameters) {
        if (!type.ContainsGenericParameters) {
            tb.AddInterfaceImplementation(type);
            return;
        }
        
        tb.AddInterfaceImplementation(type.MakeGenericType(generic_parameters));
        var gparams = type.GetGenericArguments();
        Dictionary<Type, Type> gparam2ParamMap = new(gparams.Length);
        for(int i = 0; i < gparams.Length; i++)
            gparam2ParamMap.Add(gparams[i], generic_parameters[i]);

        foreach (var it in type.GetInterfaces()) {
            if (!it.ContainsGenericParameters) 
                continue;

            var replacedGenerics = it.GetGenericArguments().Select(x => gparam2ParamMap[x]);
            tb.AddInterfaceImplementation(it.GetGenericTypeDefinition().MakeGenericType(replacedGenerics.ToArray()));
        }
        
    }
}