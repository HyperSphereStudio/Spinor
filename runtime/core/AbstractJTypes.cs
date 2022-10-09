using System;
using runtime.core.JIL;

namespace runtime.core;

[Flags]
public enum JExprFlags : byte {
    None = 0
}

[Flags]
public enum JMethodFlags : byte {
    None = 0,
    Inline = 1 << 0
}

[Flags]
public enum JModuleFlags : byte
{
    None = 0,
    Bare = 1 << 0
}

[Flags]
public enum JFieldFlags : byte{
    None = 0,
    Const = 1 << 0,
    Global = 1 << 1,
}

public enum JTypeType : byte{
    None,
    Abstract,
    Mutable,
    Struct,
    Primitive
}



public interface IJCodeContext {
    public string GetString(JStringRef i);
    public IJModule GetCtxModule(int i);
    public IJType GetCtxType(int i);
    public JStringRef GetStringIndex(string s);
    public bool GetNameRef(IJExpr e, string name, out JNameRef nameRef);
    public IJField GetNameField(IJExpr e, JNameRef nameRef);
}

public interface IJCodeExecutionContext : IJCodeContext{
    public void EnterModule(IJModule m);
    public void ExitModule();
    public void EnterExpr(IJExpr e);
    public void ExitExpr();
    public IJExpr GetExpr(int i);
    public IJModule CurrentModule { get; }
    public IJExpr CurrentExpr { get; }
}

public interface IJModule : IJExpr
{
    public string Name { get; }
    IJModule IJExpr.Module => this;
    public IJModule ParentModule => Parent.Module;
    public JModuleFlags ModuleModifiers { get; }
    public bool IsBare => ModuleModifiers.HasFlag(JModuleFlags.Bare);
    public bool GetNameV<T>(JNameRef r, out T t);
    public bool GetName(JNameRef r, out object o) => GetNameV<object>(r, out o);
}

public interface IJExpr {
    public JExprFlags Modifiers { get; }
    public IJExpr Parent { get; }
    public IJModule Module => Parent.Module;
    public IJCodeContext Context => Parent.Context;
    public bool VisitVariables(Func<IJField, bool> v);
    public bool VisitNames(Func<IJField, object, bool> v);
    public IJField GetNameField(JNameRef nameRef) => Context.GetNameField(this, nameRef);
    internal IJField GetNameFieldImpl(JNameRef nameRef);
    
    public bool GetNameRef(string name, out JNameRef nameRef) => Context.GetNameRef(this, name, out nameRef);
    internal bool GetNameRefImpl(string name, out JNameRef nameRef);
    
    
    public JNameRef GetNameRef(string name) {
        if (GetNameRef(name, out var nameRef))
            return nameRef;
        throw new JuliaException("Unable to Create Name Reference \"" + name + "\"");
    }
}

public interface IJMethod : IJExpr{
    public JMethodFlags MethodModifiers { get; }
    public bool VisitParameters(Func<IJField, bool> v);
    public bool ShouldInline => MethodModifiers.HasFlag(JMethodFlags.Inline);
}

public interface IJType : IJExpr{
    public string Name { get; }
    public JTypeType Type { get; }
    public bool VisitFields(Func<IJField, bool> v);
    public bool VisitConstructors(Func<IJMethod, bool> v);
}

public interface IJField {
    public string Name { get; }
    public IJType Type { get; }
    public JFieldFlags Modifiers { get; }
    public IJExpr Parent { get; }

    public bool IsConst => Modifiers.HasFlag(JFieldFlags.Const);

    public bool IsGlobal => Modifiers.HasFlag(JFieldFlags.Global);

    public bool IsLocal => !IsGlobal;
}

public interface IJFunction {
    public string Name { get; }

    public bool VisitMethods(Func<IJMethod, bool> v);
}

public interface IJObject
{
    public IJType GetJType();
}

public interface IJName : IJField {
    public object ObjectValue { get; set; }
    public JNameRef NameRef { get; }
    IJExpr IJField.Parent => Type.Parent;
}

public interface IJName<T> : IJName
{
    public T Value { get; set; }
}