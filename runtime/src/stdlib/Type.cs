using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using runtime.core;

namespace Core;

[Flags]
public enum FieldProperties : byte {
    Const,
    Atomic,
    Normal
}

public struct Field {
    public Symbol Name;
    public Type Type;
    public FieldProperties Properties;

    public Field(Symbol name, Type type, FieldProperties properties) {
        Name = name;
        Type = type;
        Properties = properties;
    }
    
    public Field(string name, Type type, FieldProperties properties) {
        Name = (Symbol) name;
        Type = type;
        Properties = properties;
    }
}


[Flags]
public enum TypeProperties : byte{
    Abstract = 1,
    Mutable = 2,
    MayInlineAlloc = 4,
    Primitive = 5,
}

public interface Type : Any
{
    public Symbol Name { get; }
    public Module Module { get; }
    public IList<Field> Fields { get; }
    public TypeProperties Properties { get; }

    public bool Isa(Type t);
    public bool Isa(Any v) => Isa(v.Type);
    
    public bool IsAbstract => Properties.HasFlag(TypeProperties.Abstract);
    public bool IsMutable => Properties.HasFlag(TypeProperties.Mutable);
    public bool MayInlineAlloc => Properties.HasFlag(TypeProperties.MayInlineAlloc);
    public bool IsConcreteType { get; }
}



public class DataType : Type
{
    public static readonly DataType[] EmptyTypes = new DataType[0];
    public static readonly Field[] EmptyFields = new Field[0];
    
    public Symbol Name { get; init; }
    public Module Module { get; init; }
    public IList<Field> Fields { get; init; }
    public TypeProperties Properties { get; init; }
    public Type Type => Types.DataType;
    public DataType Super { get; internal set; }
    public DataType[] Parameters { get; init; }
    public bool IsConcreteType => Parameters.Length == 0;
    public object Instance { get; internal set; }
    public int Hash { get; init; }

    public override int GetHashCode() => Hash;
    internal static DataType NewUnitializedDataType() => new();

    public static DataType NewDataType(Symbol name, Module m, DataType super, DataType[] parameters, Field[] fields, TypeProperties properties) {
        var t = new DataType {
            Name = name,
            Module = m,
            Fields = fields,
            Super = super,
            Parameters = parameters,
            Properties = properties,
            Hash = HashCode.Combine(HashCode.Combine(m != null ? m.BuildID.Lo : 0, name.Hash), 0xa1ada1da)
        };
        return t;
    }

    public static DataType NewAbstractType(Symbol name, Module m, DataType super, DataType[] parameters) => NewDataType(name, m, super, parameters, Array.Empty<Field>(), TypeProperties.Abstract);

    public bool Isa(Type t) {
        if (t == this || this == Core.Types.Any)
            return true;
        if (t == Core.Types.Type)
            return true;
        return false;
    }
}

public static class Types
{
    public static DataType
        Module, Any, Type, DataType, Symbol, Nothing, SystemType, 
        AbstractString, Function, Builtin;
        
    public static PrimitiveSystemType 
        Int32, UInt32, 
        Int64, UInt64,
        Int16, UInt16,
        Int8, UInt8,
        Bool, String;


    internal static void Init()
    {
        Any = DataType.NewAbstractType(CommonSymbols.Any, Modules.Core, null, DataType.EmptyTypes);
        Any.Super = Any;

        Symbol = DataType.NewDataType((Symbol)"Symbol", Modules.Core, Any, DataType.EmptyTypes, DataType.EmptyFields,
            TypeProperties.Mutable);
        Type = DataType.NewAbstractType(CommonSymbols.Type, Modules.Core, Any, DataType.EmptyTypes);

        // NOTE: types are not actually mutable, but we want to ensure they are heap-allocated with stable addresses
        DataType = DataType.NewDataType((Symbol) "DataType", Modules.Core, Type, 
            DataType.EmptyTypes, new Field[] {}, TypeProperties.Mutable);
     
        Nothing = DataType.NewDataType((Symbol) "Nothing", Modules.Core, Any, DataType.EmptyTypes, DataType.EmptyFields, 0);
        Nothing.Instance = Nothing;

        UInt8 = new PrimitiveSystemType((Symbol) "UInt8", typeof(byte));
        Int8 = new PrimitiveSystemType((Symbol) "Int8",typeof(sbyte));
        UInt16 = new PrimitiveSystemType((Symbol) "UInt16",typeof(ushort));
        Int16 = new PrimitiveSystemType((Symbol) "Int16",typeof(short));
        UInt32 = new PrimitiveSystemType((Symbol) "UInt32",typeof(uint));
        Int32 = new PrimitiveSystemType((Symbol) "Int32",typeof(int));
        UInt64 = new PrimitiveSystemType((Symbol) "UInt64",typeof(ulong));
        Int64 = new PrimitiveSystemType((Symbol) "Int64",typeof(long));
        Bool = new PrimitiveSystemType((Symbol) "Bool",typeof(bool));
        
        AbstractString =  DataType.NewAbstractType((Symbol) "AbstractString", Modules.Core, Any, DataType.EmptyTypes);
        String = new PrimitiveSystemType((Symbol) "String",typeof(string));
        
        Function = DataType.NewAbstractType((Symbol) "Function", Modules.Core, Any, DataType.EmptyTypes);
        Builtin  = DataType.NewAbstractType((Symbol) "Builtin", Modules.Core, Function, DataType.EmptyTypes);
        
        Module = DataType.NewDataType((Symbol) "Module", Modules.Core, Any, DataType.EmptyTypes, DataType.EmptyFields, TypeProperties.Mutable);

        SystemType = DataType.NewDataType((Symbol)"SystemType", Modules.Core, Type, DataType.EmptyTypes, DataType.EmptyFields, TypeProperties.Mutable);
    }
}