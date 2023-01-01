using System;
using Core;
using runtime.core;
using runtime.core.expr;

namespace sandbox
{
    public static class Program {

        static void Main(string[] args) {
            Spinor.Init();
            try {
                var p = new ExprParser();
                var expr = (Expr) p.Parse(@"
                            module MyModule
                                x = 5
                                x *= 2
                                x = (x + 2) * (x - 2)

                                struct MyStruct 
                                        field1
                                end
                            end");
                
                expr.WriteCode(Console.Out);
                expr.PrintLn();
            }
            catch (SpinorException e) {
                e.Print();
            }
            Spinor.Exit();
        }
        
    }
}