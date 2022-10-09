using System;
using System.Collections.Generic;
using runtime.core.JIL;
using runtime.Utils;

namespace runtime.core.Runtime;

public sealed class JRuntimeJILMethod : JILMethod{
    internal JRuntimeJILMethod(JILMethod j, JRuntimeModule parent) : 
        base(j.MethodModifiers, j.Parameters, j.Modifiers, j.Code, j.VarTable, j.Names, parent) {}
}

public sealed class JRuntimeFunction : IJFunction
{
    private readonly string name;
    private readonly List<IJMethod> _methods;
    public string Name => name;
    
    internal JRuntimeFunction(string name, IJMethod[] mts) {
        name = name;
        _methods = new(mts);
    }

    public bool VisitMethods(Func<IJMethod, bool> v) => _methods.Visit(v);

    public object Invoke(object[] parameters)
    {
        return null;
    }
}