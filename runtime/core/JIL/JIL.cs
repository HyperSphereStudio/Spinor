global using JStringRef = System.Int32;
global using JMemoryRef = System.Int32;
using System;

namespace runtime.core.JIL;

public readonly struct JNameRef {
    public readonly int HeightToExprName;
    public readonly int ExprNameIndex;

    internal JNameRef(int heightToExprName, int exprNameIndex) {
        HeightToExprName = heightToExprName;
        ExprNameIndex = exprNameIndex;
    }
}

public readonly struct JILSimpleVec<T> where T: unmanaged{
    public readonly JMemoryRef Index;

    public JILSimpleVec(JMemoryRef i) => Index = i;
    
    public unsafe T* GetPtr(IJCodeContext ctx, out int length) {
        var ptr = ctx.GetMemory<int>(Index);
        length = *ptr++;
        return (T*)ptr;
    }
}

public struct JILExpr {
    public readonly JExprFlags Modifiers;
    public readonly JILSimpleVec<JOp> Code;
    
    public readonly JILSimpleVec<JILField> VarTable;

    internal JILExpr(JExprFlags modifiers, JILSimpleVec<JOp> code, JILSimpleVec<JILField> variables) {
        Modifiers = modifiers;
        Code = code;
        VarTable = variables;
    }
}

public readonly unsafe struct JILExprReader : IJExpr {
    private readonly JILExpr* _expr;
    private readonly IJCodeContext _ctx;

    public JILExprReader(JILExpr* expr, IJCodeContext ctx) {
        _expr = expr;
        _ctx = ctx;
    }
    
    public JExprFlags Modifiers => _expr->Modifiers;

    public JILField* GetField(JNameRef r) {
        var name = _ctx.GetNameName(r);
        var ptr = _expr->VarTable.GetPtr(_ctx, out int len);

        for (int i = 0; i < len; i++) {
            var p = ptr++;
            if (name == p->Name)
                return p;
        }
        
        return null;
    }

    public void VisitVariables(Action<IJField> v) {
        var ptr = _expr->VarTable.GetPtr(_ctx, out int len);
        var fr = new JILFieldReader(ptr, _ctx);
        for (int i = 0; i < len; i++, fr.MoveNext())
            v(fr);
    }
}

public struct JILMethod {
    public readonly JMethodFlags MethodModifiers;
    public readonly JILSimpleVec<JILField> Parameters;
    public readonly JILExpr Expr;

    internal JILMethod(JMethodFlags methodModifiers, JILSimpleVec<JILField> parameters, JILExpr expr){
        MethodModifiers = methodModifiers;
        Parameters = parameters;
        Expr = expr;
    }
    
    public bool ShouldInline => MethodModifiers.HasFlag(JMethodFlags.Inline);
}

public unsafe struct JILMethodReader : IJMethod{
    private JILMethod* _expr;
    private readonly IJCodeContext _ctx;

    public JILMethodReader(JILMethod* expr, IJCodeContext ctx) {
        _expr = expr;
        _ctx = ctx;
    }

    public JILExpr* Expr => &_expr->Expr;
    public JExprFlags Modifiers => Expr->Modifiers;
    public JMethodFlags MethodModifiers => _expr->MethodModifiers;

    public void MoveNext() => _expr++;

    public void VisitVariables(Action<IJField> v) => new JILExprReader(Expr, _ctx).VisitVariables(v);
    
    public void VisitParameters(Action<IJField> v) {
        var ptr = _expr->Parameters.GetPtr(_ctx, out int len);
        var fr = new JILFieldReader(ptr, _ctx);
        for (int i = 0; i < len; i++, fr.MoveNext())
            v(fr);
    }
}

public struct JILField{
    public readonly JStringRef Name;
    public readonly JFieldFlags Modifiers;
    public JNameRef Type;

    internal JILField(JStringRef name, JFieldFlags modifiers, JNameRef type)
    {
        Name = name;
        Modifiers = modifiers;
        Type = type;
    }
}

public unsafe class JILFieldReader : IJField{
    private JILField* _baseField;
    private readonly IJCodeContext _ctx;

    public JILFieldReader(JILField* fr, IJCodeContext ctx) {
        _baseField = fr;
        _ctx = ctx;
    }

    internal void MoveNext() => _baseField++;

    public string Name => _ctx.GetString(_baseField->Name);
    public IJType Type => _ctx.GetNameV<IJType>(_baseField->Type);
    public JFieldFlags Modifiers => _baseField->Modifiers;
}

public struct JILType{
    public readonly JStringRef Name;
    public readonly JTypeType Type;
    public readonly JILSimpleVec<JILField> Fields;
    public readonly JILSimpleVec<JILMethod> Constructors;

    internal JILType(JStringRef name, JTypeType type, JILSimpleVec<JILField> fields, JILSimpleVec<JILMethod> constructors) {
        Name = name; 
        Type = type;
        Fields = fields;
        Constructors = constructors;
    }
}

public readonly unsafe struct JILTypeReader : IJType
{
    private readonly JILType* _baseType;
    private readonly IJCodeContext _ctx;

    public JILTypeReader(JILType* fr, IJCodeContext ctx) {
        _baseType = fr;
        _ctx = ctx;
    }

    public string Name => _ctx.GetString(_baseType->Name);
    public JTypeType Type => _baseType->Type;
    public void VisitFields(Action<IJField> v) {
        var ptr = _baseType->Fields.GetPtr(_ctx, out int len);
        var fr = new JILFieldReader(ptr, _ctx);
        for (int i = 0; i < len; i++, fr.MoveNext())
            v(fr);
    }

    public void VisitConstructors(Action<IJMethod> v) {
        var ptr = _baseType->Constructors.GetPtr(_ctx, out int len);
        var mr = new JILMethodReader(ptr, _ctx);
        for (int i = 0; i < len; i++, mr.MoveNext())
            v(mr);
    }
}

public sealed class JILModule : IJModule
{
    private readonly JStringRef _name;
    private readonly IJCodeContext _ctx;
    private readonly JModuleFlags _mflags;
    private readonly JILExpr _moduleExpr;

    internal JILModule(JStringRef name, IJCodeContext ctx, JModuleFlags flags, JILExpr moduleExpr) {
        _name = name;
        _ctx = ctx;
        _mflags = flags;
        _moduleExpr = moduleExpr;
    }

    public string Name => _ctx.GetString(_name);
    public JModuleFlags ModuleModifiers => _mflags;
    public IJCodeContext Context => _ctx;
}