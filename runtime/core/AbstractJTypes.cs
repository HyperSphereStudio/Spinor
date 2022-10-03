using System;
using runtime.core.JIL;
using runtime.Utils;

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

public interface IJCodeContext
{
    public unsafe T* GetMemory<T>(JMemoryRef index) where T : unmanaged;
    public string GetString(JStringRef i);
    public JStringRef GetStringIndex(string s);
    public IJModule CurrentExecutingModule { get; set; }
    public IJExpr CurrentExecutingExpr { get; set; }
    public JStringRef GetNameName(JNameRef r);
    public object GetNameV(JNameRef r);
    public T GetNameV<T>(JNameRef r) => (T) GetNameV(r);
}

public interface IJModule
{
    public string Name { get; }
    public JModuleFlags ModuleModifiers { get; }
    public IJCodeContext Context { get; }
    public bool IsBare => ModuleModifiers.HasFlag(JModuleFlags.Bare);
}

public interface IJExpr {
    public JExprFlags Modifiers { get; }
    public void VisitVariables(Action<IJField> v);
}

public interface IJMethod : IJExpr{
    public JMethodFlags MethodModifiers { get; }
    public void VisitParameters(Action<IJField> v);
    public bool ShouldInline => MethodModifiers.HasFlag(JMethodFlags.Inline);
}

public interface IJType {
    public string Name { get; }
    public JTypeType Type { get; }

    public void VisitFields(Action<IJField> v);
    public void VisitConstructors(Action<IJMethod> v);
}

public interface IJField {
    public string Name { get; }
    public IJType Type { get; }
    public JFieldFlags Modifiers { get; }

    public bool IsConst => Modifiers.HasFlag(JFieldFlags.Const);

    public bool IsGlobal => Modifiers.HasFlag(JFieldFlags.Global);

    public bool IsLocal => !IsGlobal;
}

public interface IJFunction {
    public string Name { get; }

    public void VisitMethods(Action<IJMethod> v);
}

public interface IJObject
{
    public IJType GetJType();
}

public interface IJName : IJField {
    public object ObjectValue { get; set; }
    public JNameRef NameRef { get; }
}

public interface IJName<T> : IJName
{
    public T Value { get; set; }
}