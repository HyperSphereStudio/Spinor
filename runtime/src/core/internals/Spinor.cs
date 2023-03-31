/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Core;
using runtime.core.parse;
using runtime.core.type;
using runtime.stdlib;

namespace runtime.core.internals;

public enum SpinorPhase : byte {
    Initializing,
    Bootstrapping,
    Running,
    Destroying,
    Destroyed
}

public static class Spinor {
    static Spinor() {
        TopModules = new();
        Types = new();
        Boot.Launch();
    }
    
    public static SpinorPhase ProgramPhase => Boot.ProgramPhase;
    public static AbstractType Exception { get; internal set; }
    public static Any Nothing { get; internal set; }
    public static Module Core { get; internal set; }
    public static Module Base { get; internal set; }
    public static CompileTimeTopModule Main { get; internal set; } 
    public static CompileTimeTopModule Root { get; internal set; }
    internal static readonly Dictionary<Symbol, Module> TopModules;
    internal static readonly Dictionary<Type, SType> Types;

    public static SType Int64 => System<long>.RuntimeType;
    public static SType Float64 => System<double>.RuntimeType;
    public static SType Bool => System<bool>.RuntimeType;
    #region JLPort
    public static Symbol sp_symbol(string s) => Symbol.Create(s);
    public static unsafe Symbol sp_symbol(char* str) => sp_symbol(new string(str));
    public static unsafe Symbol sp_symbol_n(char* str, int len) => sp_symbol(new string(new ReadOnlySpan<char>(str, len)));
    public static string sp_symbol_str(Symbol s) => s.String;
    #endregion

    public static bool UnboxBool(Any a) => (bool) a;
    public static long UnboxInt64(Any a) => (long) a;
    public static double UnboxFloat64(Any a) => (double)a;

    public static Any BoxBool(bool b) => b;
    public static Any BoxInt64(long b) => b;
    public static Any BoxFloat64(double b) => b;

    public static Any Box(object o) {
        return o switch {
            null => default,
            ISpinorAny sp => new Any(sp),
            ISystemAny sys => new Any(sys),
            _ => (Any) Reflect.Any_SystemBox1.MakeGenericMethod(o.GetType()).Invoke(null, new[]{o})
        };
    }
   
    public static void Exit() => Boot.ProgramPhase = SpinorPhase.Destroyed;
    public static Any Eval(Any a) => Root.Evaluate(a);
    public static Any Parse(string str) => new ExprParser().Parse(str);
    public static Any ParseFile(string file) => new ExprParser().Parse(new FileInfo(file));
    public static Any EvalFromStr(string str) => Eval(Parse(str));
    public static Any EvalFromFile(string file) => Eval(ParseFile(file));

    public static SType SType(this Type t) {
        if (Types.TryGetValue(t, out var s))
            return s;
        return (SType) typeof(System<>).MakeGenericType(t).GetMethod("get_RuntimeType").Invoke(null, Array.Empty<object>());
    }
    
    public static Any BoxObject(this object o) {
        return o switch {
            null => Any.RuntimeType,
            Any k => k,
            _ => o switch {
                ISpinorAny a => new(a),
                ISystemAny s => new(s),
                _ => (Any)Reflect.Any_Box1.MakeGenericMethod(o.GetType()).Invoke(null, new[] { o })
            }
        };
    }
}