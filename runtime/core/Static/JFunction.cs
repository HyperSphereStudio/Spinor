using System.Reflection.Emit;
using runtime.core.Abstract;
using runtime.core.Dynamic.JIL;

namespace runtime.core.Static
{
    public struct JParameter {
        public string Name;
        public IJType Type;

        public JParameter(string name, IJType type) {
            Name = name;
            Type = type;
        }
    }

    public struct JLocal{
        public string Name;
        public LocalBuilder LocalBuilder;
        public IJType Type;

        public JLocal(string name, IJType type, LocalBuilder localBuilder) {
            Name = name;
            LocalBuilder = localBuilder;
            Type = type;
        }
    }
    
    public class JFunction {
        public readonly string Name;
        public readonly IJType Parent, ReturnType;
        public readonly JParameter[] Arguments;
        public readonly JLocal[] Locals;
        public readonly JOp[] ILCode;

        internal JFunction(string name, IJType parent, IJType returnType, JParameter[] arguments, JLocal[] locals, JOp[] ilCode) {
            Name = name;
            Parent = parent;
            ReturnType = returnType;
            Arguments = arguments;
            ILCode = ilCode;
            Locals = locals;
        }

        public bool GetParameter(string s, out JParameter v) {
            foreach(var a in Arguments)
                if (a.Name == s) {
                    v = a;
                    return true;
                }
            v = default;
            return false;
        }

        public bool GetLocal(string s, out JLocal v)
        {
            foreach(var a in Locals)
                if (a.Name == s) {
                    v = a;
                    return true;
                }
            v = default;
            return false;
        }
    }
}