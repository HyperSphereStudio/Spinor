using System;
using runtime.core.JIL;

namespace runtime.core.Runtime;

public class JRuntimeExpr : IJExpr
{
    private readonly JExprFlags _modifiers;
    public readonly byte[] Code;
    public readonly JNameRef[] VarTable;
    public readonly JILField[] Names;
    internal IJExpr iparent;

    internal JRuntimeExpr(JILExpr e, IJExpr parent) {
        Code = e.Code;
        VarTable = e.VarTable;
        Names = e.Names;
        iparent = parent;
    }

    public JExprFlags Modifiers => _modifiers;
    IJExpr IJExpr.Parent => iparent;

    public bool VisitVariables(Func<IJField, bool> v) {
        foreach (var va in VarTable) {
            if (!v(Names[va.CompileTimeNameRefIndex]))
                return false;
        }
        return false;
    }
    
    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < Names.Length; i++)
            if (!v(Names[i], null))
                return false;
        return true;
    }
    
    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => Names[nameRef.CompileTimeNameRefIndex];

    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef) => throw new NotImplementedException();

    public override string ToString() {
        JILPrinter p = new();
        JILPrinter.PrintCode(iparent.Context, Code, p);
        return p.ToString();
    }
}