using System.Collections.Generic;

namespace Core;

public class Symbol
{
    private static readonly Dictionary<string, Symbol> intern_strings = new();
    public readonly string String;
    public readonly int Hash;

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
        Empty = (Symbol)"",
        Call = (Symbol)"call",
        Invoke = (Symbol)"invoke",
        InvokeModify = (Symbol)"invoke_modify",
        ForeignCall = (Symbol)"foreigncall",
        CFunction = (Symbol)"cfunction",
        Quote = (Symbol)"quote",
        Inert = (Symbol)"inert",
        Top = (Symbol)"top",
        Core = (Symbol)"core",
        GlobalRef = (Symbol)"globalref",
        Line = (Symbol)"line",
        LineInfo = (Symbol)"lineinfo",
        Incomplete = (Symbol)"incomplete",
        Error = (Symbol)"error",
        Goto = (Symbol)"goto",
        GotoIfNot = (Symbol)"gotoifnot",
        Lambda = (Symbol)"lambda",
        Module = (Symbol)"module",
        Export = (Symbol)"export",
        Return = (Symbol)"return",
        Import = (Symbol)"import",
        Using = (Symbol)"using",
        Assign = (Symbol)"=",
        Method = (Symbol)"method",
        Exeception = (Symbol)"the_exception",
        Enter = (Symbol)"enter",
        Leave = (Symbol)"leave",
        New = (Symbol)"new",
        SplatNew = (Symbol)"splatnew",
        Const = (Symbol)"const",
        Global = (Symbol)"global",
        Thunk = (Symbol)"thunk",
        TopLevel = (Symbol)"toplevel",
        Dot = (Symbol)".",
        As = (Symbol)"as",
        Colon = (Symbol)":",
        BoundsCheck = (Symbol)"boundscheck",
        InBounds = (Symbol)"inbounds",
        NewVar = (Symbol)"newvar",
        CopyAST = (Symbol)"copyast",
        loopinfo = (Symbol)"loopinfo",
        Pure = (Symbol)"pure",
        Meta = (Symbol)"meta",
        List = (Symbol)"list",
        Unused = (Symbol)"#unused#",
        Slot = (Symbol)"slot",
        StaticParameter = (Symbol)"static_parameter",
        Inline = (Symbol)"inline",
        NoInline = (Symbol)"noinline",
        PropagateInbounds = (Symbol)"propagate_inbounds",
        AggressiveConstProp = (Symbol)"aggressive_constprop",
        Purity = (Symbol)"purity",
        IsDefined = (Symbol)"isdefined",
        NoSpecialize = (Symbol)"nospecialize",
        Specialize = (Symbol)"specialize",
        OptLevel = (Symbol)"optlevel",
        Compile = (Symbol)"compile",
        ForceCompile = (Symbol)"force_compile",
        Infer = (Symbol)"infer",
        MaxMethods = (Symbol)"max_methods",
        MacroCall = (Symbol)"macrocall",
        Escape = (Symbol)"escape",
        HygienicScope = (Symbol)"hygienic-scope",
        GCPreserveBegin = (Symbol)"gc_preserve_begin",
        GCPreserveEnd = (Symbol)"gc_preserve_end",
        Generated = (Symbol)"generated",
        GeneratedOnly = (Symbol)"generated_only",
        ThrowUndefIfNot = (Symbol)"throw_undef_if_not",
        GetFieldUndefRef = (Symbol)"##getfield##",
        Do = (Symbol)"do",
        ThisModule = (Symbol)"thismodule",
        Block = (Symbol)"block",
        Statement = (Symbol)"statement",
        All = (Symbol)"all",
        Atomic = (Symbol)"atomic",
        NotAtomic = (Symbol)"not_atomic",
        Unordered = (Symbol)"unordered";
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