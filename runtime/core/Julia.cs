using System.IO;
using runtime.core.Dynamic;

namespace runtime.core
{
    public class Julia {
        public static JExecutionModule MAIN = JExecutionModule.CreateExecutionModule("Main");
        public static JExecutionModule BASE = JExecutionModule.CreateExecutionModule("Base");
        
     //   public static object Eval(string s) => MAIN.Eval(s);
      //  public static object Eval(FileInfo file) => MAIN.Eval(file);
        
        
    }
}