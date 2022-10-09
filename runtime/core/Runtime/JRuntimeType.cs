using System;
using System.Linq;
using runtime.core.JIL;
using runtime.Utils;

namespace runtime.core.Runtime;

public class JRuntimeName : IJName {
    private readonly IJField _field;
    private object _v;
    private JNameRef _nameRef;

    public JRuntimeName(IJField field, JNameRef nameRef, object v = null) {
        _field = field;
        _v = v;
        _nameRef = nameRef;
    }

    public object Value { get => _v; set => _v = value; }
    public object ObjectValue { get => _v; set => _v = value; }
    public string Name => _field.Name;
    public IJType Type => _field.Type;
    public JFieldFlags Modifiers => _field.Modifiers;
    public JNameRef NameRef => _nameRef;
}
    
public class JRuntimeType : IJType {
    private readonly string _name;
    private readonly JTypeType _type;
    public readonly JILField[] Fields;
    internal IJModule iparent;
    private readonly JRuntimeFunction _constructors;

    public string Name => _name;
    public JTypeType Type => _type;
    public JExprFlags Modifiers => JExprFlags.None;
    public IJExpr Parent => iparent;
    public bool VisitFields(Func<IJField, bool> v) => Fields.Visit(x => v(x));
    public bool VisitConstructors(Func<IJMethod, bool> v) => _constructors.VisitMethods(v);

    internal JRuntimeType(JILType type) {
        _type = type.Type;
        _name = type.Name;
        Fields = type.Fields;
        _constructors = new(type.Name, type.Constructors.Select(x => (IJMethod) new JRuntimeJILMethod(x, null)).ToArray());
    }

    public bool VisitVariables(Func<IJField, bool> v) => Fields.Visit(x => v(x));
    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < Fields.Length; i++)
            if (!v(Fields[i], null))
                return false;
        return true;
    }

    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => Fields[nameRef.CompileTimeNameRefIndex];
    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef) => throw new NotImplementedException();
}