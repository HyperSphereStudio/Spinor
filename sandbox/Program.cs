/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using Core;
using runtime.core;
using runtime.core.Compilation;
using runtime.core.Compilation.compiler;

namespace sandbox
{
    public static class Program {

        static void Main(string[] args) {
            Spinor.Init();
            try
            {
                var p = new ExprParser();
                var k = new ExprSerializer();
                var e = (Expr) p.Parse(@"
                            module MyModule
                                x = 5
                                x *= 2
                                x = (x + 2) * (x - 2) * (x ^ 2) ∈ w
                                println((2, 3, x))

                                struct MyStruct 
                                       field1
                                end
                            end");
               e.WriteCode(Console.Out);
               var comp = new CLRCompiler();
               comp.Compile(e, Modules.Core);
            }
            catch (SpinorException e) {
                e.Print();
            }
            Spinor.Exit();
        }
        
    }
}