using System;
using System.Collections.Generic;
using System.Reflection;
using Core;
using runtime.core.Utils;
using Module = Core.Module;
using Type = Core.Type;

namespace runtime.core;

public class SystemFieldView : IArray<Field>
{
    public readonly Type Type;
    public readonly FieldInfo[] Fields;
    public int Count => Fields.Length;
    
    public SystemFieldView(Type type, System.Type ty) {
        Type = type;
        Fields = ty.GetFields(BindingFlags.Instance | BindingFlags.Public);
    }

    public IEnumerator<Field> GetEnumerator() => new IListEnumerator<Field>(this);
    public Field this[int index] {
        get {
            var f = Fields[index];
            return new((Symbol) f.Name, Type, f.IsInitOnly ? FieldProperties.Const : FieldProperties.Normal);
        }
        set => throw new NotImplementedException();
    }
}

public class SystemType : SystemAny, Type {
    private static readonly Dictionary<System.Type, Type> SystemTypes = new();
    private static readonly Dictionary<string, Module> SystemModules = new();

    public readonly System.Type UnderlyingSystemType;

    public Symbol Name { get; }
    public Module Module { get; }
    public override Type Type => Types.SystemType;
    public Type Super => GetOrCreateSystemType(UnderlyingSystemType.BaseType, null, Module);
    public IList<Field> Fields => new SystemFieldView(this, UnderlyingSystemType);
    public TypeProperties Properties {
        get {
            TypeProperties p = 0;
            if (UnderlyingSystemType.IsPrimitive)
                p |= TypeProperties.Primitive;
            if (UnderlyingSystemType.IsClass)
                p |= TypeProperties.Mutable;
            if (UnderlyingSystemType.IsAbstract || UnderlyingSystemType.IsInterface)
                p |= TypeProperties.Abstract;
            return p;
        }
    }
    
    private SystemType(Symbol name, System.Type t, Module m) {
        UnderlyingSystemType = t;
        Name = name ?? (Symbol) t.Name;
        Module = m ?? CreateSystemModule(t);
        SystemTypes.Add(t, this);
        Module.SetConst(Name, this);
    }
    
    public bool Isa(Type t) => t == this;
    public bool IsConcreteType => true;
    public static Type GetSystemType(System.Type t) => SystemTypes.TryGetValue(t, out var v) ? v : null;
    public static Type GetOrCreateSystemType(System.Type t, Symbol name, Module m = null) => SystemTypes.TryGetValue(t, out var v) ? v : new SystemType(name, t, m);
    private static Module CreateSystemModule(System.Type t) => throw new NotSupportedException("Cannot Currently Create System Module!");
    internal static void InitPrimitiveSystemTypes() {
        SystemTypes.Add(typeof(object), Types.Any);
        SystemTypes.Add(typeof(ValueType), Types.Any);
    }
}