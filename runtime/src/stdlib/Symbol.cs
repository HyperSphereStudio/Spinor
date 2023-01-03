/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.Collections.Generic;

namespace Core;

public class Symbol : SystemAny{
    private static readonly Dictionary<string, Symbol> intern_strings = new();
    public new readonly string String;
    public new readonly int Hash;
    
    public override Type Type => Types.Module;

    private Symbol(string s) {
        String = s;
        Hash = s.GetHashCode();
    }
    
    public static Symbol Create(string str) {
        if (intern_strings.TryGetValue(str, out var v))
            return v;
        var jsym = new Symbol(string.Intern(str));
        intern_strings.Add(jsym.String, jsym);
        return jsym;
    }

    public static explicit operator Symbol(string s) => Create(s);
    public override int GetHashCode() => Hash;
    public override string ToString() => String;
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
        Assign = (Symbol) "=";
}

public static class CommonSymbols
{
    public static Symbol
        Base = (Symbol)"Base",
        Core = (Symbol)"Core",
        Main = (Symbol)"Main",
        Any = (Symbol)"Any",
        Type = (Symbol)"Type";
}