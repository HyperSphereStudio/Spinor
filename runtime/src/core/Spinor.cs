/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/
using System;
using Core;

namespace runtime.core;

public class Spinor
{

    #region JLPort
    public static Symbol jl_symbol(string s) => Symbol.Create(s);
    public static unsafe Symbol jl_symbol(char* str) => jl_symbol(new string(str));
    public static unsafe Symbol jl_symbol_n(char* str, int len) => jl_symbol(new string(new ReadOnlySpan<char>(str, len)));
    public static void jl_module_export(Module from, Symbol s) => from.Export(s);
    public static void jl_module_using(Module to, Module from) => to.Using(from);
    public static string jl_symbol_name(Symbol s) => s.String;
    #endregion

    public static void Init() => runtime.core.Init.Initialize();
    
    public static bool UnboxBool(Any a) => Unbox<bool>(a);
    public static int UnboxInt32(Any a) => Unbox<int>(a);
    public static long UnboxInt64(Any a) => Unbox<long>(a);
    
    public static Any BoxBool(bool b) => Box(b);
    public static Any BoxInt32(int b) => Box(b);
    public static Any BoxInt64(long b) => Box(b);


    public static void Exit(){}
    public static Any Box<T>(T o) => new SystemPrimitiveAny<T>(o);
    public static T Unbox<T>(Any a) => ((SystemPrimitiveAny<T>) a).Value;
}