using System;
using System.Collections.Generic;
using runtime.core.JIL;
using runtime.parse;
using runtime.Utils;

namespace runtime.core.Runtime;

//High Read/Write Speed for Globals
//High Read for Names
//Slow Write for Names
//Low Memory for Names & Globals
public class JRuntimeModule : JRuntimeExpr, IJModule {
    private readonly string _name;
    internal IJCodeContext _ctx;
    private readonly JModuleFlags _mflags;
    private readonly MInternContainer<string> _names;
    private readonly List<IJName> names;

    internal JRuntimeModule(JILModule m, JRuntimeContext ctx, JRuntimeModule parent) : base(m, parent){
        _ctx = ctx;
        _name = m.Name;
        names = new(m.Names.Length);
        for (int i = 0; i < names.Count; i++)
            names[i] = new JRuntimeName(m.Names[i], new(i, 0));
    }

    public string Name => _name;
    public JModuleFlags ModuleModifiers => _mflags;
    public bool GetNameV<T>(JNameRef r, out T t) { throw new NotImplementedException(); }
    public IJCodeContext Context => _ctx;
    public JRuntimeExpr EvalToExpression(string s) => new JuliaStaticCompiler().Compile(new JuliappParser(s), this);
}