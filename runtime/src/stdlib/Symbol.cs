/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.Collections.Generic;
using System.IO;

namespace Core;

public sealed class Symbol : IAny{
    private static readonly Dictionary<string, Symbol> InternStrings = new();
    
    public readonly string String;
    private readonly int _hash;
    public static SType RuntimeType { get; set; }
    public SType Type => RuntimeType;

    private Symbol(string s) {
        String = s;
        _hash = s.GetHashCode();
    }
    
    public static Symbol Create(string str) {
        if (InternStrings.TryGetValue(str, out var v))
            return v;
        var jsym = new Symbol(string.Intern(str));
        InternStrings.Add(jsym.String, jsym);
        return jsym;
    }

    public static explicit operator Symbol(string s) => Create(s);
    public override int GetHashCode() => _hash;
    public override string ToString() => String;
    public void Print(TextWriter tw) => tw.Write(String);

}

public static class ASTSymbols
{
    public static readonly Symbol
        Empty = (Symbol) "",
        Call = (Symbol) "call",
        Invoke = (Symbol) "invoke",
        Quote = (Symbol) "quote",
        Block = (Symbol) "block",
        Struct = (Symbol) "struct",
        Module = (Symbol) "module",
        Tuple = (Symbol) "tuple",
        Assign = (Symbol) "=",
        Primitive = (Symbol) "primitive",
        Abstract = (Symbol) "abstract",
        Extends = (Symbol) "<:",
        Builtin = (Symbol) "builtin",
        Colon = (Symbol) "::";
}

public static class CommonSymbols
{
    public static readonly Symbol
        Base = (Symbol) "Base",
        Core = (Symbol) "Core",
        Main = (Symbol) "Main",
        Any = (Symbol) "Any",
        Type = (Symbol) "Type",
        Union = (Symbol) "Union",
        TypeVar = (Symbol) "TypeVar",
        Integer = (Symbol) "Integer",
        AbstractFloat = (Symbol) "AbstractFloat",
        Signed = (Symbol) "Signed",
        Unsigned = (Symbol) "Unsigned",
        Exception = (Symbol) "Exception";
}