/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using runtime.core;
using runtime.core.type;
using runtime.ILCompiler;
using runtime.Utils;

namespace Core;

public abstract class SType : IAny<SType> {
    private static readonly Dictionary<Type, SType> CachedSystem2SpinorTypes = new();
    private static readonly Dictionary<SType, Type> CachedSpinor2SystemTypes = new();
    
    public static SType NothingType { get; internal set; }
    public static SType RuntimeType { get; set; }
    public Symbol Name { get; }
    public AbstractType Super { get; internal set; }
    public Module Module { get; }
    public Type UnderlyingType { get; private set; }
    public SType This => this;
    public void Print(TextWriter tw) => tw.Write(Name.String);
    
    public virtual bool IsMutable => true;
    public virtual bool IsReference => true;
    public virtual bool IsSystemWrapper => false;
    public virtual bool IsUnmanagedType => false;
    public virtual int StackLength => 8;
    public virtual int HeapLength => 0;


    protected SType(Symbol name, AbstractType super, Module module, Type underlyingType) {
        Name = name;
        Super = super;
        UnderlyingType = underlyingType;
        Module = module;

        //Build Basic Type Object
        if (underlyingType is not TypeBuilder tb) 
            return;
        
        ((RuntimeModule) module).SetConst(name, this);
    }

    public static SType GetType(Type ty, RuntimeModule rm = null) => CachedSystem2SpinorTypes.TryGetValue(ty, out var v) ? v : RegisterSystemType(ty, CreateTypeFromSystem(ty, rm));

    private static SType RegisterSystemType(Type sty, SType st) {
        CachedSpinor2SystemTypes.Add(st, sty);
        CachedSystem2SpinorTypes.Add(sty, st);
        return st;
    }

    private static AbstractType CreateSuperType(Type t, Type super, RuntimeModule rm) => super == null ? Any.RuntimeType : (AbstractType) GetType(t.BaseType, rm);

    public static SType CreateTypeFromSystem(Type t, RuntimeModule rm = null) {
        if (CachedSystem2SpinorTypes.TryGetValue(t, out var v))
            return v;
        
        rm ??= Spinor.Root;
        SType ty;
        AbstractType super;
        Symbol name;
        PropertyInfo pi = null;

        if (t.IsAssignableTo(typeof(IAny))) { 
            pi = t.GetProperty("RuntimeType", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            ty = (SType) pi.GetValue(null);
          
            if (ty != null) //Already Instantiated Runtime Type
                return ty;
            
            name = (Symbol) t.Name;
            super = CreateSuperType(t, null, rm);
        }else {
            name = (Symbol) t.Name;
            super = CreateSuperType(t, null, rm);
        }

        if (t.IsPrimitive) {
            ty = new PrimitiveType(name, super, rm, t, Marshal.SizeOf(t));
        }else if (t.IsClass) {
            ty = new MutableStructType(name, super, rm, t);
        }else if (t.IsInterface) {
            ty = new AbstractType(name, super, rm, t, BuiltinType.None);
        }
        else throw new NotImplementedException();
        
        rm.SetConst(name, ty);
        pi?.SetValue(null, ty);
        return ty;
    }

    protected static void ImplementIAnyInterface(TypeBuilder ty, Type ianyT) {
        ty.AddInterfaceImplementation(ianyT);
        ty.CreateBackingGetSetProperty("RuntimeType", typeof(SType), Attributes.Static | Attributes.Public);

        var mb = new IlExprBuilder(ty.CreateMethod("get_This", ty, IlExprBuilder.InterfaceAttributes | MethodAttributes.Public));
        mb.Load.This();
        mb.Return();
    }

    public virtual Type Initialize() {
        try {
            if (UnderlyingType is TypeBuilder tb)
                UnderlyingType = tb.CreateType();
            return UnderlyingType;
        }
        catch (Exception e) {
            "Error while Creating Type {0}".PrintLn(Name);
            throw e;
        }
    }
    
    public static bool IsUnmanaged(Type type) {
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            return true;
        
        if (!type.IsValueType)
            return false;
        
        return type
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .All(f => IsUnmanaged(f.FieldType));
    }
}

public sealed class MutableStructType : SType {
    public const TypeAttributes MutableStructAttributes = TypeAttributes.Interface | TypeAttributes.Public | TypeAttributes.Abstract;
    
    internal MutableStructType(Symbol name, AbstractType super, Module module, Type underlyingType) : 
        base(name, super, module, underlyingType) {}
    
    internal static MutableStructType Create(Symbol name, AbstractType super, RuntimeModule module) => 
        Create(name, super, (super ?? Any.RuntimeType).UnderlyingType, module);

    internal static MutableStructType Create(Symbol name, AbstractType ssuper, Type super, RuntimeModule module) => 
        new(name, ssuper, module, module.ModuleBuilder.DefineType(name.String, MutableStructAttributes, null, new[]{super}));
}
