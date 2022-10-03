using System;
using System.Collections.Generic;
using runtime.core.Containers;
using runtime.core.JIL;

namespace runtime.core.Dynamic;

public class JRuntimeExpr : IJExpr
{
    protected readonly JILExpr internalExpr;
    protected readonly JRuntimeExpr parentExpr;
    protected readonly ushort _exprDepth;

    public JExprFlags Modifiers { get => internalExpr.Modifiers; set => throw new NotSupportedException(); }
    public IJExpr ParentContextExpr => parentExpr is IJModule ? null : parentExpr;
    public IJExpr Parent => parentExpr;
    public int ExprDepth => _exprDepth;

    internal JRuntimeExpr(JILExpr e, JRuntimeExpr parentExpr) {
        _exprDepth = (ushort) (parentExpr.ExprDepth + e.ExprDepth);
        this.parentExpr = parentExpr;
    }

    public void VisitLocalNames(Action<IJVar> v) => parentExpr.VisitLocalNames(v);

    bool IJExpr.GetNameImpl(string name, out IJVar v, bool throwOnError) {
        
    }

    IJVar IJExpr.CreateNameImpl(IJField f, bool throwOnError) {
        
    }
}