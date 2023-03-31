using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using runtime.ILCompiler;

namespace runtime.core.compilation.ILCompiler;

using static MethodAttributes;

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
    public IlExprBuilder CreateCastOperator(Type foreignType, bool isExplicit, bool convertToSource) =>
        InternalBuilder.CreateCastOperator(foreignType, isExplicit, convertToSource);

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
        
        RequiredGetSetAttributes = HideBySig | SpecialName,
        
        RequiredConstructorAttributes = HideBySig | SpecialName;

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
        
        eb.Load.Field(fb);
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
        for(var i = 0; i < gparams.Length; i++)
            gparam2ParamMap.Add(gparams[i], generic_parameters[i]);

        foreach (var it in type.GetInterfaces()) {
            if (!it.ContainsGenericParameters) 
                continue;

            var replacedGenerics = it.GetGenericArguments().Select(x => gparam2ParamMap[x]);
            tb.AddInterfaceImplementation(it.GetGenericTypeDefinition().MakeGenericType(replacedGenerics.ToArray()));
        }
        
    }
    
    public static IlExprBuilder CreateCastOperator(this TypeBuilder sourceType, Type foreignType, bool isExplicit, bool convertToSource)
    {
        var mb = sourceType.CreateMethod(isExplicit ? "op_Explicit" : "op_Implicit",
            convertToSource ? sourceType : foreignType,
            Public | Static | SpecialName | HideBySig,
            convertToSource ? foreignType : sourceType);
        
        mb.DefineParameter(1, ParameterAttributes.None, "x");
        return mb;
    }
    
    public static MethodInfo GetCastOperator(this Type sourceType, Type foreign, bool isExplicit, bool convertToSource) {
        var name = isExplicit ? "op_Explicit" : "op_Implicit";
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
        return convertToSource ? sourceType.GetMethod(name, flags, new[] { foreign }) : sourceType.GetMethods(flags).First(mi => mi.Name == name && mi.ReturnType == foreign);
    }

    public static Type GetTupleType(int startIdx = 0, params Type[] t) {
        var typeCount = Math.Min(8, t.Length - startIdx);
        if (typeCount == 0)
            return typeof(ValueTuple);

        var typeChunks = new Type[typeCount];
        for (var i = 0; i < typeCount; i++)
            typeChunks[i] = t[i + startIdx];

        if (typeCount <= 7)
            return typeCount switch {
                1 => typeof(ValueTuple<>).MakeGenericType(typeChunks),
                2 => typeof(ValueTuple<,>).MakeGenericType(typeChunks),
                3 => typeof(ValueTuple<,,>).MakeGenericType(typeChunks),
                4 => typeof(ValueTuple<,,,>).MakeGenericType(typeChunks),
                5 => typeof(ValueTuple<,,,,>).MakeGenericType(typeChunks),
                6 => typeof(ValueTuple<,,,,,>).MakeGenericType(typeChunks),
                7 => typeof(ValueTuple<,,,,,,>).MakeGenericType(typeChunks)
            };
        
        typeChunks[7] = GetTupleType(7, typeChunks);
        return typeof(ValueTuple<,,,,,,,>).MakeGenericType(typeChunks);
    }
}