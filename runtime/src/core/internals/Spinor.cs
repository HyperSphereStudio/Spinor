/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/
using System;
using System.IO;
using System.Reflection.Emit;
using Core;
using runtime.core.Compilation;
using runtime.core.type;
using runtime.ILCompiler;

namespace runtime.core;

public enum SpinorPhase : byte {
    Initializing,
    BootstrappingFake,
    BootstrappingReal,
    Running,
    Destroying,
    Destroyed
}

public class Spinor
{
    public static SpinorPhase ProgramPhase { get; internal set; } = SpinorPhase.Initializing;
    public static NumericType Int64 { get; internal set; }
    public static NumericType Float64 { get; internal set; }
    public static NumericType Bool { get; internal set; }
    public static AbstractType Signed { get; internal set; }
    public static AbstractType Unsigned { get; internal set; }
    public static AbstractType AbstractFloat { get; internal set; }
    public static AbstractType Exception { get; internal set; }
    public static Module Core { get; internal set; }
    public static Module Base { get; internal set; }
    public static RuntimeTopModule Main { get; internal set; } 
    public static RuntimeTopModule Root { get; internal set; }
    
    
    #region JLPort
    public static Symbol sp_symbol(string s) => Symbol.Create(s);
    public static unsafe Symbol sp_symbol(char* str) => sp_symbol(new string(str));
    public static unsafe Symbol sp_symbol_n(char* str, int len) => sp_symbol(new string(new ReadOnlySpan<char>(str, len)));
    public static string sp_symbol_name(Symbol s) => s.String;
    #endregion

    public static void Init() => core.Init.Initialize();

    public static bool UnboxBool(Any a) => Unbox<bool>(a);
    public static long UnboxInt64(Any a) => Unbox<long>(a);
    public static double UnboxFloat64(Any a) => Unbox<double>(a);

    public static Any BoxBool(bool b) => Box(b);
    public static Any BoxInt64(long b) => Box(b);
    public static Any BoxFloat64(double b) => Box(b);

    public static void Exit() => ProgramPhase = SpinorPhase.Destroyed;

    public static Any Box<T>(T t) => BoxedPrimitiveType<T>.Boxing(t);
    public static T Unbox<T>(Any a) => BoxedPrimitiveType<T>.Unboxing(a);
    
    public static Any Eval(Any a) => Root.Evaluate(a);
    public static Any Parse(string str) => new ExprParser().Parse(str);
    public static Any ParseFile(string file) => new ExprParser().Parse(new FileInfo(file));
    public static Any EvalFromStr(string str) => Eval(Parse(str));
    public static Any EvalFromFile(string file) => Eval(ParseFile(file));
}

internal static class BoxedPrimitiveType<T> {
    public delegate Any Boxer(T t);
    public delegate T UnBoxer(Any a);

    public static Boxer Boxing { get; private set; }
    public static UnBoxer Unboxing { get; private set; }

    internal static void Create(PrimitiveType ty) {
        var ity = ty.UnderlyingType;
        
        var dynamic = new DynamicMethod($"__Boxing[{ty.Name}]__", typeof(Any), new[]{typeof(T)}, ity);
        var ex = new IlExprBuilder(dynamic);
        ex.Load.Arg(0);
        ex.Function.Invoke(ty.Constructor);
        ex.Box(ity);
        ex.Return();
        Boxing = dynamic.CreateDelegate<Boxer>();

        dynamic = new DynamicMethod($"__UnBoxing[{ty.Name}]__", typeof(T), new[]{typeof(Any)}, ity);
        ex = new IlExprBuilder(dynamic);
        ex.Load.This();
        ex.Unbox(ity);
        
        if (ty is NumericType nty)
            ex.Load.FieldValue(nty.ValueField);
        
        ex.Return();
        Unboxing = dynamic.CreateDelegate<UnBoxer>();
    }
}