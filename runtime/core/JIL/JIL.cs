global using JStringRef = System.Int32;
using System;
using runtime.Utils;

namespace runtime.core.JIL;

public readonly struct JNameRef {
    public readonly int CompileTimeNameRefIndex;
    public readonly ushort CompileTimeExprStackDelta;

    public JNameRef(int nameRefIndex, ushort stackDelta) {
        CompileTimeNameRefIndex = nameRefIndex;
        CompileTimeExprStackDelta = stackDelta;
    }
}

public class JILExpr : IJExpr {
    private readonly JExprFlags _modifiers;
    public readonly byte[] Code;
    public readonly JNameRef[] VarTable;
    public readonly JILField[] Names;
    internal IJExpr iparent;
    
    public JExprFlags Modifiers => _modifiers;
    public IJExpr Parent => iparent;

    public bool VisitVariables(Func<IJField, bool> v) {
        foreach (var va in VarTable) {
            if (!v(Names[va.CompileTimeNameRefIndex]))
                return false;
        }
        return true;
    }

    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < Names.Length; i++)
            if (!v(Names[i], null))
                return false;
        return true;
    }
    

    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => Names[nameRef.CompileTimeNameRefIndex];
    
    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef) => throw new NotImplementedException();

    internal JILExpr(JExprFlags modifiers, byte[] code, JNameRef[] variables, JILField[] names, IJExpr iparent)
    {
        _modifiers = modifiers;
        Code = code;
        VarTable = variables;
        Names = names;
        this.iparent = iparent;
    }
}

public class JILMethod : JILExpr, IJMethod
{
    private readonly JMethodFlags _methodModifiers;
    public readonly JNameRef[] Parameters;

    public JMethodFlags MethodModifiers => _methodModifiers;
    public bool VisitParameters(Func<IJField, bool> v) {
        var names = Names;
        foreach (var va in Parameters) {
            if (!v(names[va.CompileTimeNameRefIndex]))
                return false;
        }
        return true;
    }

    public bool ShouldInline => _methodModifiers.HasFlag(JMethodFlags.Inline);

    internal JILMethod(JMethodFlags methodModifiers, JNameRef[] parameters, 
            JExprFlags modifiers, byte[] code, JNameRef[] variables, JILField[] names, IJExpr iparent) : base(modifiers, code, variables, names, iparent) {
        _methodModifiers = methodModifiers;
        Parameters = parameters;
    }
}

public struct JILField : IJField {
    public readonly JStringRef NameRef;
    private readonly JFieldFlags _modifiers;
    public readonly JNameRef TypeRef;
    internal IJExpr iparent;

    public string Name => iparent.Context.GetString(NameRef);
    public IJType Type => throw new NotSupportedException();
    public JFieldFlags Modifiers => _modifiers;
    public IJExpr Parent => iparent;

    internal JILField(JStringRef name, JFieldFlags modifiers, JNameRef type, IJExpr iparent) {
        NameRef = name;
        _modifiers = modifiers;
        TypeRef = type;
        this.iparent = iparent;
    }
}

public class JILType : IJType{
    public readonly JStringRef NameRef;
    private readonly JTypeType _type;
    public readonly JILField[] Fields;
    public readonly JILMethod[] Constructors;
    internal IJModule iparent;

    internal JILType(JStringRef name, JTypeType type, JILField[] fields, JILMethod[] constructors, IJModule iparent) {
        NameRef = name;
        _type = type;
        Fields = fields;
        Constructors = constructors;
        this.iparent = iparent;
    }

    public string Name => iparent.Context.GetString(NameRef);
    public JTypeType Type => _type;
    public JExprFlags Modifiers => JExprFlags.None;
    public IJExpr Parent => iparent;
    public IJModule Module => iparent;
    public bool VisitVariables(Func<IJField, bool> v) => true;

    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => Fields[nameRef.CompileTimeNameRefIndex];
    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef) => throw new NotImplementedException();
    public bool VisitFields(Func<IJField, bool> v) => Fields.Visit(x => v(x));
    public bool VisitConstructors(Func<IJMethod, bool> v) => Constructors.Visit(x => v(x));
    
    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < Fields.Length; i++)
            if (!v(Fields[i], null))
                return false;
        return true;
    }
}

public sealed class JILModule : JILExpr, IJModule
{
    private readonly JStringRef _name;
    internal IJCodeContext _ctx;
    private readonly JModuleFlags _mflags;

    internal JILModule(JStringRef name, JModuleFlags flags, 
        JExprFlags modifiers, byte[] code, JNameRef[] variables, JILField[] names, IJExpr iparent) : base(modifiers, code, variables, names, iparent) {
        _name = name;
        _mflags = flags;
        this.iparent = iparent;
    }

    public string Name => _ctx.GetString(_name);
    
    public JModuleFlags ModuleModifiers => _mflags;
    public bool GetNameV<T>(JNameRef r, out T t) => throw new NotSupportedException();
}