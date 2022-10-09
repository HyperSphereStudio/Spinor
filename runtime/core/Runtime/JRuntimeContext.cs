using System;
using System.Collections.Generic;
using System.Linq;
using runtime.core.JIL;

namespace runtime.core.Runtime;

public class JRuntimeContext : IJCodeExecutionContext
{

    private string[] _strings;
    private IJModule[] _modules;
    private IJType[] _types;

    private int _exprStackEnd;
    private int _moduleStackEnd;
    private readonly List<IJExpr> _expressionStack = new();
    private readonly List<IJModule> _moduleStack = new();

    public JRuntimeContext(string[] strings, IJModule[] modules, IJType[] types) {
        _strings = strings;
        _modules = modules;
        _types = types;
    }
    public string GetString(int i) => _strings[i];
    public IJModule GetCtxModule(int i) => _modules[i];
    public IJType GetCtxType(int i) => _types[i];
    public IJExpr GetExpr(int i) => _expressionStack[i];
    public IJModule GetModule(int i) => _moduleStack[i];
    public int GetStringIndex(string s) => throw new NotImplementedException();

    public bool GetNameRef(IJExpr e, string name, out JNameRef nameRef) {
        for(int i = _exprStackEnd; i >= 0; i --)
            if (_expressionStack[i].GetNameRefImpl(name, out nameRef))
                return true;
        nameRef = default;
        return false;
    }

    public IJField GetNameField(IJExpr e, JNameRef nameRef) => _expressionStack[_exprStackEnd - nameRef.CompileTimeExprStackDelta].GetNameFieldImpl(nameRef);

    public void MergeContext(IJCodeContext cctx, string[] strings, IJModule[] modules, IJType[] types)
    {
        lock (_strings) {
            var oldStrLen = _strings.Length;
            Array.Resize(ref _strings, oldStrLen + strings.Length);
            strings.CopyTo(_strings, oldStrLen);
            
            var oldModLen = _modules.Length;
            Array.Resize(ref _modules, oldModLen + modules.Length);
            modules.CopyTo(_modules, oldModLen);
            
            var oldTyLen = _types.Length;
            Array.Resize(ref _types, oldTyLen + types.Length);
            types.CopyTo(_types, oldTyLen);

            foreach (var m in modules)
                (m as JILModule)._ctx = this;
        }
    }

    IJModule IJCodeExecutionContext.CurrentModule => _moduleStack.First();
    IJExpr IJCodeExecutionContext.CurrentExpr => _expressionStack.First();

    void IJCodeExecutionContext.EnterModule(IJModule m)
    {
        _moduleStack.Add(m);
        _moduleStackEnd++;
    }

    void IJCodeExecutionContext.ExitModule() => _moduleStack.RemoveAt(_moduleStackEnd--);

    void IJCodeExecutionContext.EnterExpr(IJExpr e) {
        _expressionStack.Add(e);
        _exprStackEnd++;
    }
    void IJCodeExecutionContext.ExitExpr() => _expressionStack.RemoveAt(_exprStackEnd--);
}