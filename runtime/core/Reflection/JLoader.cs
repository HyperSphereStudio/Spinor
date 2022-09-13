using runtime.core.Abstract;
using runtime.core.Dynamic;

namespace runtime.core.Reflection
{
    public class JLoader
    {
        public static IJModule GetModule(string name, bool throwError = false) {
            var names = name.Split(".");
            IJModule m = JExecutionModule.GetExecutionModule(names[0]);
            
            //TODO Implement Static Sharp Loading
            if (m == null)
                return null;

            if (names.Length < 2)
                throw new JuliaException("Type Name Must Contain Module!");

            for (int i = 1; i < names.Length; i++)
                if (m.GetGlobal(names[i], out JGlobalHandle<IJModule> v, throwError))
                    m = v.Data;
                else 
                    throw new JuliaException("Module \"" + names[i] + "\" Does Not Exist!");

            return m;
        }
        
        public static IJType GetType(string name, bool throwError = false) {
            var names = name.Split(".");
            IJModule m = JExecutionModule.GetExecutionModule(names[0]);
            
            //TODO Implement Static Sharp Loading
            if (m == null)
                return null;

            if (names.Length < 2)
                throw new JuliaException("Type Name Must Contain Module!");

            for (int i = 1; i < names.Length; i++)
                if (m.GetGlobal(names[i], out JGlobalHandle<IJModule> v, throwError))
                    m = v.Data;
                else 
                    throw new JuliaException("Module \"" + names[i] + "\" Does Not Exist!");
       
            m.GetGlobal(names[names.Length - 1], out JGlobalHandle<IJType> type, throwError);
            return type.Data;
        }
    }
}