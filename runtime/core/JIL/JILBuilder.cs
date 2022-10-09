global using JStructFieldRef = System.Int32;
global using JConstructorRef = System.Int32;
global using JContextTypeRef = System.Int32;
global using JContextModuleRef = System.Int32;
using System;
using System.Collections.Generic;
using System.Linq;
using runtime.core.Runtime;
using runtime.Utils;

namespace runtime.core.JIL;

public class JILILGenerator
{
    public readonly UnsafeStream Code = new();

    private unsafe void WriteS<T>(byte p, T data) where T : unmanaged
    {
        Code.Write(p);
        Code.Write(data);
    }

    public void InstantiateType(JILTypeBuilder tb) => WriteS(JOp.LoadTypeOp, tb.CtxTypeIndex);

    internal byte[] Create() => Code.ToByteArray();
}

public class JILExprBuilder : IJExpr
{
    private JExprFlags _modifiers = JExprFlags.None;
    public readonly JILILGenerator Code = new();
    protected readonly IJExpr _parent;
    protected readonly MInternContainer<string> _namesnames = new();
    protected readonly List<JILFieldBuilder> _names = new();
    protected readonly List<JILFieldBuilder> _vars = new();

    public IJExpr Parent => _parent;

    internal JILExprBuilder(IJExpr parent) => _parent = parent;

    public JNameRef AddVariable(JILFieldBuilder f)
    {
        var var = AddName(f.Name);
        _vars.Add(f);
        return var;
    }

    public JNameRef AddName(string name, bool isConst = false, bool isGlobal = false)
    {
        JILFieldBuilder fb = new(name, this);
        fb.IsConst = isConst;
        fb.IsGlobal = isGlobal;
        return AddName(fb);
    }

    public JNameRef AddName(JILFieldBuilder fb)
    {
        var idx = _namesnames.GetIndex(fb.Name);
        if (idx != -1)
        {
            if (_names[idx].IsConst)
                throw new JuliaException("Cannot redefine constant \"" + fb.Name + "\"");
        }

        _names.Add(fb);
        return new(_namesnames.Load(fb.Name), 0);
    }

    public JExprFlags Modifiers
    {
        get => _modifiers;
        set => _modifiers = value;
    }

    public bool VisitVariables(Func<IJField, bool> v) => _vars.Visit(x => v(x));
    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < _names.Count; i++)
            if (!v(_names[i], null))
                return false;
        return true;
    }

    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => _names[nameRef.CompileTimeNameRefIndex];

    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef)
    {
        var nf = _namesnames.GetIndex(name);
        if (nf != -1)
        {
            nameRef = new(nf, 0);
            return true;
        }

        nameRef = default;
        return false;
    }

    internal JILExpr Create()
    {
        var code = Code.Create();
        var vt = _vars.Select(x => new JNameRef(_namesnames.GetIndex(x.Name), 0)).ToArray();
        var names = _names.Select(x => x.Create()).ToArray();
        JILExpr e = new(_modifiers, code, vt, names, null);
        for (int i = 0; i < names.Length; i ++)
            names[i].iparent = e;
        return e;
    }
}

public struct JILFieldBuilder : IJField
{
    private readonly string _name = null;
    private readonly IJExpr _parent;
    private JFieldFlags _modifiers = JFieldFlags.None;
    public JNameRef TypeRef = default;

    internal JILFieldBuilder(string name, IJExpr parent)
    {
        _name = name;
        _parent = parent;
    }

    internal JILField Create() =>
        new((_parent.Context as JILContextWriter).WriteString(_name), _modifiers, TypeRef, null);

    public string Name => _name;

    public IJType Type => throw new JuliaException();

    public JFieldFlags Modifiers
    {
        get => _modifiers;
        set => _modifiers = value;
    }

    public IJExpr Parent => _parent;

    public bool IsConst
    {
        get => Modifiers.HasFlag(JFieldFlags.Const);
        set => Modifiers = Modifiers.Set(JFieldFlags.Const, value);
    }

    public bool IsGlobal
    {
        set => Modifiers = Modifiers.Set(JFieldFlags.Global, value);
    }

    public bool IsLocal
    {
        set => IsGlobal = !value;
    }

    public override int GetHashCode() => _name.GetHashCode();
}

public sealed class JILMethodBuilder : JILExprBuilder, IJMethod
{
    private JMethodFlags _methodModifiers = JMethodFlags.None;
    private readonly List<JILFieldBuilder> _parameters = new();

    internal JILMethodBuilder(IJExpr parent) : base(parent)
    {
    }

    public JNameRef AddParameter(JILFieldBuilder f)
    {
        _parameters.Add(f);
        return AddName(f.Name);
    }

    internal new JILMethod Create()
    {
        var code = Code.Create();
        var vt = _vars.Select(x => new JNameRef(_namesnames.GetIndex(x.Name), 0)).ToArray();
        var names = _names.Select(x => x.Create()).ToArray();
        var pt = _parameters.Select(x => new JNameRef(_namesnames.GetIndex(x.Name), 0)).ToArray();
        JILMethod m = new(_methodModifiers, pt, Modifiers, code, vt, names, null);

        for (int i = 0; i < names.Length; i ++)
            names[i].iparent = m;

        return m;
    }

    public JMethodFlags MethodModifiers
    {
        get => _methodModifiers;
        set => _methodModifiers = value;
    }

    public bool VisitParameters(Func<IJField, bool> v) => _parameters.Visit(x => v(x));
}

public class JILModuleBuilder : JILExprBuilder, IJModule
{
    private readonly string _name;
    private readonly JILContextWriter _cw;
    public readonly JContextModuleRef CtxModuleIndex;
    private JModuleFlags _moduleModifiers = JModuleFlags.None;

    public string Name => _name;
    IJCodeContext IJExpr.Context => _cw;

    public JModuleFlags ModuleModifiers
    {
        get => _moduleModifiers;
        set => _moduleModifiers = value;
    }
    
    IJModule IJModule.ParentModule => _parent.Module;

    public bool IsBare {
        set => ModuleModifiers = ModuleModifiers.Set(JModuleFlags.Bare, value);
    }

    public bool GetNameV<T>(JNameRef r, out T t) => throw new NotImplementedException();

    public JILTypeBuilder DefineType(string name)
    {
        var tb = new JILTypeBuilder(name, this);
        AddName(name, true);
        return tb;
    }

    public JILMethodBuilder DefineMethod(string name)
    {
        var mb = new JILMethodBuilder(this);
        AddName(name, true);
        return mb;
    }

    public JILModuleBuilder DefineModule(string name)
    {
        var mb = new JILModuleBuilder(name, this, _cw);
        AddName(name, true);
        return mb;
    }

    internal new JILModule Create()
    {
        var code = Code.Create();
        var vt = _vars.Select(x => new JNameRef(_namesnames.GetIndex(x.Name), 0)).ToArray();
        var names = _names.Select(x => x.Create()).ToArray();
        JILModule m = new(_cw.WriteString(Name), _moduleModifiers, Modifiers, code, vt, names, null);
        for (int i = 0; i < names.Length; i ++)
            names[i].iparent = m;
        return m;
    }

    bool IJExpr.GetNameRef(string name, out JNameRef nameRef) {
        nameRef = AddName(name);
        return true;
    }

    internal JILModuleBuilder(string name, IJModule parent, JILContextWriter cw) : base(parent)
    {
        _name = name;
        _cw = cw;
        CtxModuleIndex = _cw.WriteCtxModule(this);
    }
}

public sealed class JILTypeBuilder : IJType
{
    private JTypeType _type = JTypeType.None;
    private readonly string _name;
    public readonly JContextTypeRef CtxTypeIndex;
    private readonly List<JILFieldBuilder> _fields = new();
    private readonly List<JILMethodBuilder> _constructors = new();
    private readonly IJModule _parent;
    
    IJModule IJExpr.Module => _parent;

    internal JILTypeBuilder(string name, IJModule parent)
    {
        _name = name;
        CtxTypeIndex = ((JILContextWriter)parent.Context).WriteCtxType(this);
        _parent = parent;
    }

    public string Name => _name;

    public JTypeType Type
    {
        get => _type;
        set => _type = value;
    }

    public bool VisitFields(Func<IJField, bool> v) => _fields.Visit(x => v(x));
    public bool VisitConstructors(Func<IJMethod, bool> v) => _constructors.Visit(x => v(x));

    public JNameRef AddField(JILFieldBuilder f)
    {
        var idx = _fields.Count;
        _fields.Add(f);
        return new(idx, 0);
    }

    public JNameRef AddConstructor(JILMethodBuilder c)
    {
        var idx = _constructors.Count;
        _constructors.Add(c);
        return new(-idx, 0);
    }

    internal JILType Create()
    {
        var farr = _fields.Select(x => x.Create()).ToArray();
        var carr = _constructors.Select(x => x.Create()).ToArray();
        JILType ty = new((_parent.Context as JILContextWriter).WriteString(Name), Type, farr, carr, null);
        for (int i = 0; i < farr.Length; i ++)
            farr[i].iparent = ty;
        foreach (var t in carr)
            t.iparent = ty;
        return ty;
    }

    public JExprFlags Modifiers => JExprFlags.None;
    public IJExpr Parent => _parent;
    public bool VisitVariables(Func<IJField, bool> v) => true;
    public bool VisitNames(Func<IJField, object, bool> v) {
        for(int i = 0; i < _fields.Count; i++)
            if (!v(_fields[i], null))
                return false;
        return true;
    }

    IJField IJExpr.GetNameFieldImpl(JNameRef nameRef) => _fields[nameRef.CompileTimeExprStackDelta];
    bool IJExpr.GetNameRefImpl(string name, out JNameRef nameRef) => throw new NotImplementedException();
}

internal class JILContextWriter : IJCodeContext
{
    private readonly JRuntimeModule _parentModule;
    private readonly MInternContainer<string> _strings = new();
    private readonly List<JILModuleBuilder> _modules = new();
    private readonly List<JILTypeBuilder> _types = new();

    internal JILContextWriter(JRuntimeModule parentModule) => _parentModule = parentModule;

    public JStringRef WriteString(string s) => _strings.Load(s);

    public int WriteCtxModule(JILModuleBuilder mb)
    {
        var midx = _modules.Count;
        _modules.Add(mb);
        return midx;
    }

    public JContextTypeRef WriteCtxType(JILTypeBuilder tb)
    {
        var tidx = _types.Count;
        _types.Add(tb);
        return tidx;
    }

    public void CreateRuntimeContext(out string[] strings, out IJModule[] modules, out IJType[] types) {
        strings = _strings.Data.ToArray();
        modules = _modules.Select(x => (IJModule) x.Create()).ToArray();
        types = _types.Select(x => (IJType) x.Create()).ToArray();
    }

    public string GetString(int i) => _strings.Get(i);
    public IJModule GetCtxModule(int i) => _modules[i];
    public IJType GetCtxType(int i) => _types[i];
    public int GetStringIndex(string s) => _strings.GetIndex(s);

    public bool GetNameRef(IJExpr e, string name, out JNameRef nameRef) {
        IJExpr v = e;
        while (v != null) {
            if (GetNameRef(v, name, out nameRef))
                return true;
            
            v = v.Parent;
        }
        nameRef = default;
        return false;
    }

    public IJField GetNameField(IJExpr e, JNameRef nameRef)
    {
        if (nameRef.CompileTimeExprStackDelta != 0)
            return GetNameField(e.Parent,
                new(nameRef.CompileTimeNameRefIndex, (ushort)(nameRef.CompileTimeExprStackDelta - 1)));
        return e.GetNameFieldImpl(nameRef);
    }
}

public class JILBuilder : JILModuleBuilder {
    public JILBuilder(JRuntimeModule m) : base(m.Name, m, new JILContextWriter(m)) {}

    public JRuntimeExpr CreateExpression() {
        var expr = Create();
        ((this as IJExpr).Context as JILContextWriter).CreateRuntimeContext(out var strings, out var modules, out var types);
        (Parent.Context as JRuntimeContext).MergeContext(expr._ctx, strings, modules, types);
        return new JRuntimeExpr(expr, Parent);
    }
}