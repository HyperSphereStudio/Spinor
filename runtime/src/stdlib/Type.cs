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

public interface Type : Any {
    public Symbol Name { get; }
    public Module Module { get; }
    public Type Super { get; }
    public IList<Field> Fields { get; }
    public TypeProperties Properties { get; }

    public bool Isa(Type t);
    public bool Isa(Any v) => Isa(v.Type);
    public bool IsAbstract => Properties.HasFlag(TypeProperties.Abstract);
    public bool IsMutable => Properties.HasFlag(TypeProperties.Mutable);
    public bool MayInlineAlloc => Properties.HasFlag(TypeProperties.MayInlineAlloc);
    public bool IsConcreteType { get; }
}

public class DataType : SystemAny, Type {
    public static readonly DataType[] EmptyTypes = new DataType[0];
    public static readonly Field[] EmptyFields = new Field[0];
    
    public Symbol Name { get; init; }
    public Module Module { get; init; }
    public IList<Field> Fields { get; init; }
    public TypeProperties Properties { get; init; }
    public override Type Type => Types.DataType;
    public Type Super { get; internal set; }
    public DataType[] Parameters { get; init; }
    public bool IsConcreteType => Parameters.Length == 0;
    public object Instance { get; internal set; }
    public int Hash { get; init; }

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

    public override int GetHashCode() => Hash;
    internal static DataType NewUninitializedDataType() => new();
    
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
        Any, Type, DataType, Nothing, 
        AbstractString, Function, Builtin;
        
    //Primitive Types
    public static Type 
        Int32, UInt32, 
        Int64, UInt64,
        Int16, UInt16,
        Int8, UInt8,
        Bool, String,
        
        Expr, LineNumberNode, SystemType, Symbol, Module;

    private static Type CreateTypeFromSystem(System.Type t, Symbol name = null, Module m = null) => runtime.core.SystemType.GetOrCreateSystemType(t, name, m);

    internal static void Init() {
        Any = DataType.NewAbstractType(CommonSymbols.Any, Modules.Core, null, DataType.EmptyTypes);
        Any.Super = Any;

        Symbol = CreateTypeFromSystem(typeof(Symbol), null, Modules.Core);
        Type = DataType.NewAbstractType(CommonSymbols.Type, Modules.Core, Any, DataType.EmptyTypes);

        // NOTE: types are not actually mutable, but we want to ensure they are heap-allocated with stable addresses
        DataType = DataType.NewDataType((Symbol) "DataType", Modules.Core, Type, 
            DataType.EmptyTypes, new Field[] {}, TypeProperties.Mutable);
     
        Nothing = DataType.NewDataType((Symbol) "Nothing", Modules.Core, Any, DataType.EmptyTypes, DataType.EmptyFields, 0);
        Nothing.Instance = Nothing;

        UInt8 = CreateTypeFromSystem(typeof(byte), (Symbol) "UInt8", Modules.Core);
        Int8 = CreateTypeFromSystem(typeof(sbyte), (Symbol) "Int8", Modules.Core);
        UInt16 = CreateTypeFromSystem(typeof(ushort), (Symbol) "UInt16", Modules.Core);
        Int16 = CreateTypeFromSystem(typeof(short), (Symbol) "Int16", Modules.Core);
        UInt32 = CreateTypeFromSystem(typeof(uint), (Symbol) "UInt32", Modules.Core);
        Int32 = CreateTypeFromSystem(typeof(int), (Symbol) "Int32", Modules.Core);
        UInt64 = CreateTypeFromSystem(typeof(ulong), (Symbol) "UInt64", Modules.Core);
        Int64 = CreateTypeFromSystem(typeof(long), (Symbol) "Int64", Modules.Core);
        Bool = CreateTypeFromSystem(typeof(bool), (Symbol) "Bool", Modules.Core);
        Module = CreateTypeFromSystem(typeof(Module), null, Modules.Core);
        SystemType = CreateTypeFromSystem(typeof(SystemType), null, Modules.Core);
        Expr = CreateTypeFromSystem(typeof(Expr), null, Modules.Core);
        LineNumberNode = CreateTypeFromSystem(typeof(LineNumberNode), null, Modules.Core);

        AbstractString =  DataType.NewAbstractType((Symbol) "AbstractString", Modules.Core, Any, DataType.EmptyTypes);
        String = DataType.NewDataType((Symbol) "String", Modules.Core, AbstractString, DataType.EmptyTypes, DataType.EmptyFields, TypeProperties.Mutable);
        
        Function = DataType.NewAbstractType((Symbol) "Function", Modules.Core, Any, DataType.EmptyTypes);
        Builtin  = DataType.NewAbstractType((Symbol) "Builtin", Modules.Core, Function, DataType.EmptyTypes);
    }
}