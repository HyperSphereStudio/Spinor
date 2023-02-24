using System;
using System.Reflection;
using System.Reflection.Emit;
using runtime.Utils;

namespace runtime.ILCompiler;

[Flags]
public enum Attributes {
    Public = 1,
    Private = 2,
    Static = 4,
    Constant = 8,
    None = 0,
    Delete = 16
}

public static class AttributesExtension {
    public static Attributes ClearVisibility(this Attributes a) {
        a = a.Set(Attributes.Public, false);
        return a.Set(Attributes.Private, false);
    }
    
    public static FieldAttributes ClearVisibility(this FieldAttributes a) {
        a = a.Set(FieldAttributes.Public, false);
        return a.Set(FieldAttributes.Private, false);
    }
    
    public static MethodAttributes ApplyMAttribs(this Attributes attribs) {
        MethodAttributes def = 0;
        if (attribs.HasFlag(Attributes.Private))
            def |= MethodAttributes.Private;
        else if (attribs.HasFlag(Attributes.Public))
            def |= MethodAttributes.Public;
        
        if (attribs.HasFlag(Attributes.Static))
            def |= MethodAttributes.Static;

        return def;
    }
    
    public static TypeAttributes ApplyTAttribs(this Attributes attribs) {
        TypeAttributes def = 0;
        if (attribs.HasFlag(Attributes.Public))
            def |= TypeAttributes.Public;
        else if (attribs.HasFlag(Attributes.Private))
            def |= TypeAttributes.NotPublic;
        return def;
    }
    
    public static FieldAttributes ApplyFAttribs(this Attributes attribs) {
        FieldAttributes def = 0;
        if (attribs.HasFlag(Attributes.Public))
            def |= FieldAttributes.Public;
        else if (attribs.HasFlag(Attributes.Private))
            def |= FieldAttributes.Private;
        
        if (attribs.HasFlag(Attributes.Static))
            def |= FieldAttributes.Static;
        
        if (attribs.HasFlag(Attributes.Constant))
            def |= FieldAttributes.InitOnly;
        return def;
    }
    
    public static PropertyAttributes ApplyPAttribs(this Attributes attrib) => PropertyAttributes.None;
    public static bool Delete(this Attributes attrib) => attrib.HasFlag(Attributes.Delete);
}

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
    public FieldBuilder CreateField(string name, Type type, Attributes attribs) => CreateField(name, type, attribs.ApplyFAttribs());
    public PropertyBuilder CreateProperty(string name, Type t, PropertyAttributes attribs) => InternalBuilder.CreateProperty(name, t, attribs);
    public PropertyBuilder CreateProperty(string name, Type t, Attributes attr) => InternalBuilder.CreateProperty(name, t, attr);
    public IlExprBuilder CreateMethod(string name, Type returnType, Attributes attr, params Type[] parameters) => new(InternalBuilder.CreateMethod(name, returnType, attr, parameters));

    public IlExprBuilder CreateMethod(string name, Type returnType, MethodAttributes attr, params Type[] parameters) => new(InternalBuilder.CreateMethod(name, returnType, attr, parameters));
    
    public void AddInterface(Type t) => InternalBuilder.AddInterfaceImplementation(t);
    public PropertyBuilder CreateBackingGetSetProperty(string name, Type type, PropertyAttributes patt,
        FieldAttributes fatt, MethodAttributes getatt, MethodAttributes setatt) => 
        InternalBuilder.CreateBackingGetSetProperty(name, type, patt, fatt, getatt, setatt);
    public PropertyBuilder CreateBackingGetSetProperty(string name, Type type,
        Attributes patt, Attributes fatt, Attributes getatt, Attributes setatt) =>
        InternalBuilder.CreateBackingGetSetProperty(name, type, patt, fatt, getatt, setatt);
    public PropertyBuilder CreateBackingGetSetProperty(string name, Type type, Attributes attr) =>
        InternalBuilder.CreateBackingGetSetProperty(name, type, attr);

    public IlExprBuilder CreateGetMethod(PropertyBuilder pb, Attributes attr, FieldBuilder basicGetField = null) => CreateGetMethod(pb, attr, basicGetField);
    public IlExprBuilder CreateSetMethod(PropertyBuilder pb, Attributes attr, FieldBuilder basicSetField = null) => CreateSetMethod(pb, attr, basicSetField);
    
    public IlExprBuilder CreateConstructor(MethodAttributes attr, params Type[] parameters) =>
        new(InternalBuilder.CreateConstructor(attr, parameters));

    public IlExprBuilder CreateConstructor(Attributes attr, params Type[] parameters) =>
        new(InternalBuilder.CreateConstructor(attr, parameters));
    
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
    
    public static ConstructorBuilder CreateConstructor(this TypeBuilder tb, Attributes attr, params Type[] parameters) => 
        tb.CreateConstructor(attr.ApplyMAttribs(), parameters);
    
    public static IlExprBuilder CreateSetMethod(this TypeBuilder tb, PropertyBuilder pb, MethodAttributes attr, FieldBuilder basicSetField = null) {
        IlExprBuilder eb = new(tb.CreateMethod("set_" + pb.Name, pb.PropertyType, attr | RequiredGetSetAttributes, pb.PropertyType));
        pb.SetSetMethod(eb);
            
        if (basicSetField == null) 
            return eb;
        
        if(!attr.HasFlag(MethodAttributes.Static))
            eb.Load.This(false);
        
        eb.Load.Arg(0);
        eb.Store.Field(basicSetField);
        eb.ReturnVoid();

        return eb;
    }
    
    public static IlExprBuilder CreateGetMethod(this TypeBuilder tb, PropertyBuilder pb, MethodAttributes attr, FieldBuilder basicGetField = null) {
        IlExprBuilder eb = new(tb.CreateMethod("get_" + pb.Name, pb.PropertyType, attr | RequiredGetSetAttributes, Type.EmptyTypes));
        pb.SetGetMethod(eb);
            
        if (basicGetField == null) 
            return eb;
        
        if(!attr.HasFlag(MethodAttributes.Static))
            eb.Load.This(false);
        
        eb.Load.FieldValue(basicGetField);
        eb.Return();
            
        return eb;
    }
    
    public static MethodBuilder CreateMethod(this TypeBuilder tb, string name, Type returnType, MethodAttributes attr,
        params Type[] parameters) {
        var mb = tb.DefineMethod(name, attr | RequiredMethodAttributes, returnType, parameters);
        mb.SetImplementationFlags(MethodImplAttributes.Managed);
        return mb;
    }

    public static MethodBuilder CreateMethod(this TypeBuilder tb, string name, Type returnType, Attributes attr,
        params Type[] parameters) => tb.CreateMethod(name, returnType, attr.ApplyMAttribs(), parameters);
    
    public static PropertyBuilder CreateBackingGetSetProperty(this TypeBuilder tb, string name, Type type, 
            PropertyAttributes patt, FieldAttributes fatt, MethodAttributes gatt, MethodAttributes satt, bool deleteGet = false, bool deleteSet = false) {
        var pb = tb.CreateProperty(name, type, patt);
        var fb = tb.CreateField("__" + name + "__", type, fatt.ClearVisibility() | FieldAttributes.Private);
        
        if(!deleteGet)
            tb.CreateGetMethod(pb, gatt, fb);
        
        if(!deleteSet)
            tb.CreateSetMethod(pb, satt, fb);
        
        return pb;
    }

    public static PropertyBuilder CreateBackingGetSetProperty(this TypeBuilder tb, string name, Type type,
        Attributes patt, Attributes fatt, Attributes getatt, Attributes setatt) => 
        tb.CreateBackingGetSetProperty(name, type, patt.ApplyPAttribs(), fatt.ApplyFAttribs(), getatt.ApplyMAttribs(), setatt.ApplyMAttribs(), getatt.Delete(), setatt.Delete());

    public static PropertyBuilder CreateBackingGetSetProperty(this TypeBuilder tb, string name, Type type, Attributes attr)
        => tb.CreateBackingGetSetProperty(name, type, attr, attr, attr, attr);
    
    public static FieldBuilder CreateField(this TypeBuilder tb, string name, Type type, FieldAttributes attr) => 
        tb.DefineField(name, type, attr);
    public static FieldBuilder CreateField(this TypeBuilder tb, string name, Type type, Attributes attr) => 
        tb.CreateField(name, type, attr.ApplyFAttribs());
    public static PropertyBuilder CreateProperty(this TypeBuilder tb, string name, Type t, PropertyAttributes attribs) => 
        tb.DefineProperty(name, attribs, t, Type.EmptyTypes);
    public static PropertyBuilder CreateProperty(this TypeBuilder tb, string name, Type t, Attributes attr) => 
        tb.CreateProperty(name, t, attr.ApplyPAttribs());
}