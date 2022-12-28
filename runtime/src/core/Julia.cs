
using System;
using Core;

namespace runtime.core;

public class Julia
{

    public static Symbol jl_symbol(string s) => Symbol.Create(s);
    public static unsafe Symbol jl_symbol(char* str) => jl_symbol(new string(str));
    public static unsafe Symbol jl_symbol_n(char* str, int len) => jl_symbol(new string(new ReadOnlySpan<char>(str, len)));

    public static void jl_module_export(Module from, Symbol s) => from.Export(s);
    public static void jl_module_using(Module to, Module from) => to.Using(from);
    public static string jl_symbol_name(Symbol s) => s.String;

    public static void Init() => runtime.core.Init.Initialize();
    public static void Exit(){}

}