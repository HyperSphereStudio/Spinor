/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using runtime.core;
using runtime.core.internals;
using runtime.stdlib;

namespace sandbox;

public static class Program {
    static void Main(string[] args) {
        Any a = 2;
        
        Spinor.Core[(Symbol) "test"].PrintLn();
        
        Spinor.Exit();
    }
}