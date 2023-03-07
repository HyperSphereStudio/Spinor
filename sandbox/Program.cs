/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core;

namespace sandbox;

public static class Program {
    private static void ProblemCode() {
        var name = new AssemblyName("Test");
        var asm = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        var mb = asm.DefineDynamicModule("Test");
    }
    
    static void Main(string[] args) {
        try {
            Spinor.Init(); 
         
        }catch (SpinorException e) {
            e.Print();
        }
        Spinor.Exit();
    }
    
}