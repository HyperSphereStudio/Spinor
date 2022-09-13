using System;

namespace runtime.core.Dynamic.JIL
{
    public enum JOp : byte{
        Primitive, Struct, AbstractType,
        Module, Function, Var,
        
        Using,
        
        Assign, TypeOf, ModuleOf, Block,
        
        Invoke, Return
    }

    [Flags]
    public enum JVarFlags{
        Local = 0,
        Global = 1,
        Const = 2
    }
    
}