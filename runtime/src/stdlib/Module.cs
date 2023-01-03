/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using runtime.core;
using runtime.core.Compilation;
using runtime.Utils;

namespace Core;

public struct UUID
{
    public ulong Hi;
    public ulong Lo;

    public UUID()
    {
        Hi = 0;
        Lo = 0;
    }
}

[Flags]
public enum BindingFlag : byte {
    Const = 1 << 0,
    Exported = 1 << 1,
    Imported = 1 << 2,
    Renamed = 1 << 3,
    Moved = 1 << 4
}

public class GlobalRef{
    public Module Module;
    public readonly Symbol Name;
    public Binding BindCache; // Not serialized. Caches the value of jl_get_binding(mod, name).

    public GlobalRef(Module module, Symbol name, Binding bindCache) {
        Module = module;
        Name = name;
        BindCache = bindCache;
    }
}

public class Binding {
    public readonly Symbol Name;
    public Any Value;
    public GlobalRef GlobalRef; // cached GlobalRef for this binding
    public Module Owner; // for individual imported bindings
    public Type BindingType;
    public BindingFlag Flags;

    public bool Const {
        get => Flags.HasFlag(BindingFlag.Const);
        set => Flags = Flags.Set(BindingFlag.Const, value);
    }

    public bool Exported {
        get => Flags.HasFlag(BindingFlag.Exported);
        set => Flags = Flags.Set(BindingFlag.Exported, value);
    }

    public bool Imported {
        get => Flags.HasFlag(BindingFlag.Imported);
        set => Flags = Flags.Set(BindingFlag.Imported, value);
    }
    
    public bool Renamed {
        get => Flags.HasFlag(BindingFlag.Renamed);
        set => Flags = Flags.Set(BindingFlag.Renamed, value);
    }
    
    public bool Moved {
        get => Flags.HasFlag(BindingFlag.Moved);
        set => Flags = Flags.Set(BindingFlag.Moved, value);
    }
    
    public bool Deprecated => Renamed || Moved;

    public Binding(Symbol name) {
        Name = name;
        Flags = 0;
    }
    
    public static bool operator==(Binding a, Binding b) {
        if (ReferenceEquals(a, b))
            return true;
        if (a.Name == b.Name && a.Owner == b.Owner)
            return true;
        return a.Const && b.Const && a.Value != null && b.Value == a.Value;
    }
    public static bool operator!=(Binding a, Binding b) => !(a == b);

    public override bool Equals(object o) {
        if (o is Binding b)
            return this == b;
        return false;
    }

    public void DeprecationWarning() {
        // Only print a warning for deprecated == 1 (renamed).
        // For deprecated == 2 (moved to a package) the binding is to a function
        // that throws an error, so we don't want to print a warning too.
        if (!Renamed || !SpinorOptions.DependencyWarn)
            return;
        
        "WARNING: ".Err();
        if (Owner != null)
            "{0}.{1} is deprecated".ErrLn(Owner.Name, Name);
        else
            "{0} is deprecated".ErrLn(Name);
    }
}

public class Module : SystemAny {
    public Symbol Name { get; }
    public Module Parent { get; internal set; }
    public override Type Type => Types.Module;
    public readonly Dictionary<Symbol, Binding> Bindings = new();
    public readonly List<Module> Usings = new();
    public readonly UUID BuildID, UUID;
    public virtual bool IsTopMod => false;
    public readonly object Lock = new();
    private readonly int _hash;

    public Module(Symbol name, Module parent, bool defaultNames = true) {
        Name = name;
        Parent = parent;
        UUID = new();
        BuildID = new UUID { Lo = (ulong)DateTime.Now.Ticks, Hi = ~ (ulong)0 };

        _hash = parent == null
            ? HashCode.Combine(Name.Hash, Types.Module == null ? 0x12345678 : Types.Module.GetHashCode())
            : HashCode.Combine(Name.Hash, parent._hash);
        
        if (Modules.Core != null && defaultNames)
            Using(Modules.Core);

        // export own name, so "using Foo" makes "Foo" itself visible
        if (defaultNames)
            SetConst(name, this);

        Export(name);
    }

    #region ModuleManipulation

    public void Import(Module m, Symbol asName) {
        asName ??= m.Name;
        if (GetBindingResolved(asName, out var b)) {
            if (!b.Const && b.Owner != m)
                throw new SpinorException("importing {0} into {1} conflicts with an existing global", asName, m.Name);
        } else {
            b = m.GetBindingWrapper(asName, true);
            b.Imported = true;
        }
        if (!b.Const)
            b.Const = true;
    }
    public void Using(Module from)
    {
        if (this == from)
            return;
        lock (Lock)
            Usings.Add(from);
    }
    public void Export(Symbol s)
    {
        lock (Lock)
        {
            if (!Bindings.TryGetValue(s, out var b))
                b = new Binding(s);
            b.Exported = true;
        }
    }
    public GlobalRef GlobalRef(Symbol var) {
        lock (Lock) {
            if (!GetBinding(var, out var b))
                return new GlobalRef(this, var, null);
            var gr = b.GlobalRef;
            if (gr != null) 
                return gr;
            gr = new GlobalRef(this, var,
                b.Owner == null ? null : b.Owner == this ? b : b.Owner.GetBinding(b.Name));
            b.GlobalRef = gr;
            return gr;
        }
    }

    #endregion
    #region Globals

    public void SetConst(Symbol name, Any value)
    {
        var b = GetBindingWrapper(name, true);
        if (b.Value != null)
            throw new SpinorException("Invalid Redefinition of Constant {0}", b);
        b.Value = value;
        b.Const = true;
        b.BindingType = value.Type;
    }

    public void SetGlobal(Symbol var, Any val)
    {
        var b = GetBindingWrapper(var, true);
        var bt = b.BindingType;
        if (bt == Types.Any || val.Type == bt) return;
        if (!val.Isa(bt))
            "Cannot Assign an incompatible value to the Global {0}.".ErrLn(b.Name);
    }

    public Any GetGlobal(Symbol var) {
        if (!GetBinding(var, out var v))
            return null;
        if (v.Deprecated)
            v.DeprecationWarning();
        return v.Value;
    }

    #endregion
    #region Bindings

    public bool GetBinding(Symbol var, out Binding v) {
        lock (Lock)
            return Bindings.TryGetValue(var, out v);
    }
    public Binding GetBinding(Symbol var) => GetBinding(var, out var v) ? v : null;
    public bool GetBindingResolved(Symbol var, out Binding v) {
        if (GetBinding(var, out v))
            return v.Owner != null;
        return false;
    }
    public Binding GetBindingWrapper(Symbol var, bool alloc) {
        if (GetBinding(var, out var b)) {
            if (b.Owner == this)
                return b;
            if (b.Owner == null)
                b.Owner = this;
            else if (alloc)
                throw new SpinorException("Cannot Assign a value to imported variable {0}.{1} from module {2}",
                    b.Owner, var, this);
        }else if (alloc) {
            b = new Binding(var) { Owner = this };
            lock (Lock)
                Bindings.Add(var, b);
        }
        return b;
    }
    #endregion

    public TopModule TopModule {
        get {
            var m = this;
            for (;;) {
                if (m.IsTopMod)
                    return (TopModule) m;
                if (m == m.Parent)
                    break;
                m = m.Parent;
            }
            return null;
        }
    }
    public Module BaseRelativeTo(Module m) {
        for (;;) {
            if (m.IsTopMod)
                return m;
            if (m == m.Parent)
                break;
            m = m.Parent;
        }

        return Modules.TopModule;
    }
}

public class TopModule : Module, IExprLargeConstantPool {
    
    public readonly InternContainer<string> StringPool = new();
    public readonly InternContainer<Symbol> SymbolPool = new();
    public override bool IsTopMod => true;
    
    public TopModule(Symbol name, Module parent, bool defaultNames = true) : base(name, parent, defaultNames) {}
    
    public int InternString(string s) => StringPool.Load(s);
    public int InternSymbol(Symbol s) => SymbolPool.Load(s);
    public string LoadString(int i) => StringPool.Get(i);
    public Symbol LoadSymbol(int i) => SymbolPool.Get(i);
}

public static class Modules {
    public static TopModule Main, Core, Base, TopModule;
}