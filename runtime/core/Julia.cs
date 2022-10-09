using System.IO;
using runtime.core.Runtime;

namespace runtime.core
{
    public class Julia {
        public static JRootModule MAIN = JRootModule.CreateRootModule("Main");
        public static JRootModule BASE = JRootModule.CreateRootModule("Base");

        public static JRuntimeExpr EvalToExpression(string s) => MAIN.EvalToExpression(s);
       // public static object Eval(string s) => MAIN.Eval(s);
      //  public static object Eval(FileInfo file) => MAIN.Eval(file);
        
        
    }
}