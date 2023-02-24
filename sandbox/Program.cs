/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Core;
using runtime.core;
using runtime.core.Compilation;

namespace sandbox;

public interface ITest3 {}
public interface ITest2<T> : ITest3{}
public interface ITest<T> : ITest2<T> where T:ITest3{}

public static class Program {
    static void Main(string[] args) {
        Spinor.Init();
        try {
            var p = new ExprParser();
            var e = (Expr) p.Parse(new FileInfo("runtime/Core/Boot.jl"));
            e.WriteCode(Console.Out);
        }
        catch (SpinorException e) {
            e.Print();
        }
        Spinor.Exit();
    }
        
}