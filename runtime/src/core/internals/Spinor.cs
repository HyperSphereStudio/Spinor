/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/
using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Core;
using runtime.core.type;
using runtime.ILCompiler;

namespace runtime.core;

public class Spinor
{
    public static NumericType Int64 { get; internal set; }
    public static NumericType Float64 { get; internal set; }
    public static NumericType Bool { get; internal set; }
    public static AbstractType Signed { get; internal set; }
    public static AbstractType Unsigned { get; internal set; }
    public static AbstractType AbstractFloat { get; internal set; }
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

    public static void Init() => runtime.core.Init.Initialize();

    public static bool UnboxBool(Any a) => Unbox<bool>(a);
    public static long UnboxInt64(Any a) => Unbox<long>(a);

    public static Any BoxBool(bool b) => Box(b);
    public static Any BoxInt64(long b) => Box(b);
    public static Any BoxFloat64(double b) => Box(b);

    public static void Exit(){}

    public static Any Box<T>(T t) => BoxedPrimitiveType<T>.Boxing(t);
    public static T Unbox<T>(Any a) => BoxedPrimitiveType<T>.Unboxing(a);
}

internal static class BoxedPrimitiveType<T> {
    public delegate Any Boxer(T t);
    public delegate T UnBoxer(Any a);

    public static Boxer Boxing { get; private set; }
    public static UnBoxer Unboxing { get; private set; }

    internal static void Create(PrimitiveType ty) {
        ty.Initialize();

        var dynamic = new DynamicMethod(string.Empty, typeof(Any), new[]{typeof(T)}, ty.UnderlyingType);
        var ex = new IlExprBuilder(dynamic);
        ex.Create.Object(ty.Constructor);
        ex.Return();
        Boxing = dynamic.CreateDelegate<Boxer>();
        
        if (ty is not NumericType nty)
            return;
        
        dynamic = new DynamicMethod(string.Empty, typeof(T), new[]{typeof(Any)}, ty.UnderlyingType);
        ex = new IlExprBuilder(dynamic);
        ex.Load.Arg(0);
        ex.Load.FieldValue(nty.ValueField);
        ex.Return();
        Unboxing = dynamic.CreateDelegate<UnBoxer>();
    }
}