using System;
using System.Collections.Generic;
using runtime.core;
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
public enum BindingFlag : byte
{
    Const = 1 << 0,
    Exported = 1 << 1,
    Imported = 1 << 2
}

public enum Deprecation : byte
{
    NotDeprecated = 0,
    Renamed = 1,
    Moved = 2
}

public class GlobalRef{
    Module Module;
    Symbol Name;
    Binding BindCache; // Not serialized. Caches the value of jl_get_binding(mod, name).

    public GlobalRef(Module module, Symbol name, Binding bindCache) {
        Module = module;
        Name = name;
        BindCache = bindCache;
    }
}

public class Binding
{
    public Symbol Name;
    public Any Value;
    public GlobalRef GlobalRef; // cached GlobalRef for this binding
    public Module Owner; // for individual imported bindings
    public Type BindingType;
    public BindingFlag Flags;

    public bool Const
    {
        get => Flags.HasFlag(BindingFlag.Const);
        set => Flags = Flags.Set(BindingFlag.Const, value);
    }

    public bool Exported
    {
        get => Flags.HasFlag(BindingFlag.Exported);
        set => Flags = Flags.Set(BindingFlag.Exported, value);
    }

    public bool Imported
    {
        get => Flags.HasFlag(BindingFlag.Imported);
        set => Flags = Flags.Set(BindingFlag.Imported, value);
    }

    public Deprecation Deprecation
    {
        get => (Flags.ConvertTo<BindingFlag, byte>() >> 3).ConvertFrom<Deprecation, int>();
        //Clear Deprecation
        set => Flags = (Flags.ConvertTo<BindingFlag, byte>() & (1 + 2 + 4)
                        //Add New Deprecation
                        | value.ConvertTo<Deprecation, byte>()).ConvertFrom<BindingFlag, int>();
    }

    public bool Deprecated => (Flags.ConvertTo<BindingFlag, byte>() >> 3) > 0;

    public Binding(Symbol name)
    {
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
}

public class Module : SystemAny {
    public Symbol Name { get; }
    public Module Parent { get; internal set; }
    public override Type Type => Types.Module;
    
    public Dictionary<Symbol, Binding> Bindings = new();
    public readonly List<Module> Usings = new();
    public readonly UUID BuildID, UUID;
    public readonly uint PrimaryWorld;
    public int Counter;
    public bool NoSpecialize;
    public bool IsTopMod { get; private set; }
    public sbyte OptLevel;
    public sbyte Compile;
    public sbyte Infer;
    public sbyte MaxMethods;
    public readonly object Lock = new();
    public readonly int Hash;

    public Module(Symbol name, Module parent, bool defaultNames = true) {
        Name = name;
        Parent = parent;
        UUID = new();
        BuildID = new UUID { Lo = (ulong)DateTime.Now.Ticks, Hi = ~ (ulong)0 };
        PrimaryWorld = 0;
        Counter = 1;
        NoSpecialize = false;
        OptLevel = -1;
        Compile = -1;
        Infer = -1;
        MaxMethods = -1;
        IsTopMod = false;
        
        Hash = parent == null
            ? HashCode.Combine(Name.Hash, Types.Module == null ? 0x12345678 : Types.Module.GetHashCode())
            : HashCode.Combine(Name.Hash, parent.Hash);
        
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
            b = m.GetBindingWR(asName, true);
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
        var b = GetBindingWR(name, true);
        if (b.Value != null)
            throw new SpinorException("Invalid Redefinition of Constant {0}", b);
        b.Value = value;
        b.Const = true;
        b.BindingType = value.Type;
    }

    public void SetGlobal(Symbol var, Any val)
    {
        var b = GetBindingWR(var, true);
        var bt = b.BindingType;
        if (bt == Types.Any || val.Type == bt) return;
        if (!val.Isa(bt))
            "Cannot Assign an incompatible value to the Global {0}.".ErrLn(b.Name);
    }

    public Any GetGlobal(Symbol var) {
        if (!GetBinding(var, out var v))
            return null;
        if (v.Deprecated)
            jl_binding_deprecation_warning(v);
        return v.Value;
    }

    #endregion
    #region Bindings

    public bool GetBinding(Symbol var, out Binding v)
    {
        lock (Lock)
            return Bindings.TryGetValue(var, out v);
    }

    public Binding GetBinding(Symbol var)
    {
        if (GetBinding(var, out var v))
            return v;
        return null;
    }

    public bool GetBindingResolved(Symbol var, out Binding v)
    {
        if (GetBinding(var, out v))
            return v.Owner != null;
        return false;
    }

    public Binding GetBindingWR(Symbol var, bool alloc)
    {
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

    private void jl_binding_deprecation_warning(Binding b)
    {
        // Only print a warning for deprecated == 1 (renamed).
        // For deprecated == 2 (moved to a package) the binding is to a function
        // that throws an error, so we don't want to print a warning too.
        if (b.Deprecation != Deprecation.Renamed || !SpinorOptions.DependencyWarn)
            return;
        "WARNING: ".Err();
        if (b.Owner != null)
        {
            "{0}.{1} is deprecated".ErrLn(b.Owner.Name, b.Name);
        }
        else
        {
            "{0} is deprecated".ErrLn(b.Name);
        }
    }

    #endregion

    public Module BaseRelativeTo(Module m)
    {
        for (;;)
        {
            if (m.IsTopMod)
                return m;
            if (m == m.Parent)
                break;
            m = m.Parent;
        }

        return Modules.TopModule;
    }

    void SetTopMod(bool isPrimary)
    {
        IsTopMod = true;
        if (isPrimary)
            Modules.TopModule = this;
    }
}

public static class Modules
{
    public static Module Main, Core, Base, TopModule;
}