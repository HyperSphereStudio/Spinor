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

public class PrimitiveSystemType : Type
{
    public readonly System.Type UnderlyingSystemType;
    public Type Type => Types.SystemType;
    public Symbol Name { get; }
    public Module Module => Modules.Core;
    public IList<Field> Fields { get; }
    public TypeProperties Properties => TypeProperties.Primitive;

    internal PrimitiveSystemType(System.Type t) : this((Symbol) t.Name, t){}
    
    internal PrimitiveSystemType(Symbol name, System.Type t) {
        UnderlyingSystemType = t;
        Name = name;
        Fields = new SystemFieldView(this, t);
    }

    public bool Isa(Type t) => t == this;
    public bool IsConcreteType => true;
}