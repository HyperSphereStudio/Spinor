using System;
using System.Collections.Generic;
using System.Linq;
using runtime.Utils;

namespace runtime.core.JIL;

public class JILILGenerator {
    public readonly List<JOp> Code = new();

    internal JILSimpleVec<JOp> Create(JILContextWriter cw) => cw.WriteList(Code);
}

public class JILExprBuilder : IJExpr
{
    private JExprFlags _modifiers = JExprFlags.None;
    public readonly JILILGenerator Code = new();
    private readonly MInternContainer<string> _names2intmap = new();
    private readonly List<object> _names = new();
    private readonly List<JILFieldBuilder> _vars = new();

    internal JILExprBuilder() {}

    public JNameRef AddVariable(JILFieldBuilder f) {
        var var = AddName(f.Name, f);
        _vars.Add(f);
        return var;
    }

    public JNameRef AddName(string name, object o) {
        var idx = _names2intmap.Load(name);
        if (idx >= _names.Count)
            _names.Add(o);
        else _names[idx] = o;
        return new(0, idx);
    }

    public JExprFlags Modifiers {
        get => _modifiers;
        set => _modifiers = value;
    }

    public void VisitVariables(Action<IJField> v) => _vars.ForEach(x => v(x));

    internal JILExpr Create(JILContextWriter cw) {
        var code = Code.Create(cw);
        var vt = cw.WriteSimpleVec(_vars.Select(x => x.Create(cw)).ToArray());
        return new(_modifiers, code, vt);
    }
}

public struct JILFieldBuilder : IJField
{
    private readonly string _name = null;
    private JFieldFlags _modifiers = JFieldFlags.None;
    private IJType _type = null;

    internal JILFieldBuilder(string name) => _name = name;

    internal JILField Create(JILContextWriter cw) => new(cw.WriteString(_name), _modifiers, cw.WriteType(_type));

    public string Name => _name;

    public IJType Type
    {
        get => _type;
        set => _type = value;
    }

    public JFieldFlags Modifiers
    {
        get => _modifiers;
        set => _modifiers = value;
    }
    
    
    public bool IsConst { set => Modifiers = Modifiers.Set(JFieldFlags.Const, value); }
    public bool IsGlobal { set => Modifiers = Modifiers.Set(JFieldFlags.Global, value); }
    public bool IsLocal { set => IsGlobal = !value; }

    public override int GetHashCode() => _name.GetHashCode();
}

public sealed class JILMethodBuilder : JILExprBuilder, IJMethod
{
    private JMethodFlags _methodModifiers = JMethodFlags.None;
    private readonly List<JILFieldBuilder> _parameters = new();

    internal JILMethodBuilder(){}

    public JNameRef AddParameter(JILFieldBuilder f) {
        int idx = _parameters.Count;
        _parameters.Add(f);
        return new(idx);
    }

    internal JILMethod Create(JILContextWriter cw) {
        return new(_methodModifiers, 
            cw.WriteSimpleVec(_parameters.Select(x => x.Create(cw)).ToArray()),
            ((JILExprBuilder) this).Create(cw));
    }

    public JMethodFlags MethodModifiers
    {
        get => _methodModifiers;
        set => _methodModifiers = value;
    }

    public void VisitParameters(Action<IJField> v) => _parameters.ForEach(x => v(x));
}

public sealed class JILModuleBuilder : JModule
{
    private readonly string _name;
    private JModuleFlags _moduleModifiers = JModuleFlags.None;
    private readonly List<JILTypeBuilder> _declaredTypes = new();
    private readonly List<JILModuleBuilder> _declaredModules = new();
    private readonly Dictionary<string, List<JILMethodBuilder>> _declaredFunctions = new();
    private JModule _parentModule;
    internal JILContextWriter _ctx;
    public readonly JILExprBuilder Expression = new();

    public string Name => _name;

    public JModuleFlags ModuleModifiers {
        get => _moduleModifiers;
        set => _moduleModifiers = value;
    }
    
    public bool IsBare { set => ModuleModifiers = ModuleModifiers.Set(JModuleFlags.Bare, value); }

    public JILTypeBuilder DefineType(string name) {
        var tb = new JILTypeBuilder(name);
        _declaredTypes.Add(tb);
        return tb;
    }
    
    public JILMethodBuilder DefineMethod(string name) {
        List<JILMethodBuilder> methods;
        if (!_declaredFunctions.TryGetValue(name, out methods)) {
            methods = new();
            _declaredFunctions.Add(name, methods);
        }
        var mb = new JILMethodBuilder();
        methods.Add(mb);
        return mb;
    }

    public JILModuleBuilder DefineModule(string name) {
        var mb = new JILModuleBuilder(name, this, _ctx);
        _declaredModules.Add(mb);
        return mb;
    }

    public IJCodeContext Context => _parentModule.Context;

    public JModule Create() => new JILModule(_ctx.WriteString(_name), null, _moduleModifiers, Expression.Create(_ctx));

    internal JILModuleBuilder(string name, JModule parent, JILContextWriter cw) {
        _name = name;
        _parentModule = parent;
        _ctx = cw ?? new JILContextWriter();
    }
}

public sealed class JILTypeBuilder : IJType
{
    private JTypeType _type = JTypeType.None;
    private readonly string _name;
    private readonly List<JILFieldBuilder> _fields = new();
    private readonly List<JILMethodBuilder> _constructors = new();

    internal JILTypeBuilder(string name) => _name = name;
    
    public string Name => _name;

    public JTypeType Type
    {
        get => _type;
        set => _type = value;
    }

    public void VisitFields(Action<IJField> v) => _fields.ForEach(x => v(x));
    public void VisitConstructors(Action<IJMethod> v) => _constructors.ForEach(v);

    public JNameRef AddField(JILFieldBuilder f) {
        _fields.Add(f);
        
        return new(idx);
    }
    public JMethodRef AddConstructor(JILMethodBuilder c) {
        var idx = _constructors.Count;
        _constructors.Add(c);
        return new(idx);
    }

    internal unsafe JILType Create(JILContextWriter cw) {
        var farr = _fields.Select(x => x.Create(cw)).ToArray();
        var carr = _constructors.Select(x => x.Create(cw)).ToArray();
        cw.WriteArray(farr, out int farridx);
        cw.WriteArray(carr, out int carridx);
        return new(cw.WriteString(Name), Type, new(farridx), new(carridx));
    }
}

internal class JILContextWriter
{
    private readonly UnsafeStream us = new();
    private readonly MInternContainer<string> _strings = new();
    private readonly MInternContainer<int> _names = new();
    private readonly MInternContainer<JModule> _modules = new();
    private readonly MInternContainer<IJType> _types = new();

    public unsafe T* CreateMemory<T>(int n, out int index) where T : unmanaged => us.WriteArray<T>(n, out index);
    public unsafe T* WriteArray<T>(T[] v, out int index) where T: unmanaged => us.WriteArray(v, out index);
    
    public unsafe JILSimpleVec<T> WriteSimpleVec<T>(T[] v) where T:unmanaged {
        WriteArray(v, out int index);
        return new(index);
    }
    
    public JILSimpleVec<T> WriteList<T>(List<T> v) where T:unmanaged => new(us.WriteList(v));
    public JNameRef WriteName(string s) => new(_names.Load(WriteString(s).Index));
    public JStringRef WriteString(string s) => new(_strings.Load(s));
    public int WriteModule(JModule mb) => _modules.Load(mb);
    public JTypeRef WriteType(IJType mb) => new(_types.Load(mb));
    public void SetType(JTypeRef ty, IJType mb) => _types.Set(ty.Index, mb);
}