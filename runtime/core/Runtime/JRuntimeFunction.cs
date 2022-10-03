using System;
using System.Collections.Generic;
using runtime.core.Containers;
using runtime.core.JIL;

namespace runtime.core.Dynamic;

public sealed class JRuntimeMethod : JILMethod{
    internal JRuntimeMethod(JILMethod m) : base(m.Name, m.MethodModifiers, m.Parameters.ToArray(),
        m.Modifiers, m.Code.ToArray(), m.Variables.ToArray()) {}
}

public sealed class JRuntimeFunction : IJFunction
{
    private readonly string _name;
    private readonly List<JRuntimeMethod> _methods = new(1);

    internal JRuntimeFunction(JRuntimeMethod m) {
        _methods[0] = m;
        _name = m.Name;
    }

    public string Name { get => _name; set => throw new NotSupportedException(); }
    public IContainer<JRuntimeMethod> Methods => new JList<JRuntimeMethod>(_methods);
}