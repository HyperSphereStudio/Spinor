
using System;
using runtime.core;

namespace sandbox
{

    public class Program {

        static void Main(string[] args) {
            var m = Julia.MAIN;
            var rctx = m.EvalToExpression(
                @"struct s
                        x
                        x2
                    end");
            Console.WriteLine(rctx.ToString());
        }
        
    }
}