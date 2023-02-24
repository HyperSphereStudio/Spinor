/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core;
using runtime.core.type;
using runtime.ILCompiler;

namespace Core;

//Field used for internal building
public record struct Binding(FieldInfo Field, Module Module, bool Initialized, Any InternalValue = null) {
    //Runtime Value
    private Any InternalValue { get; set; } = InternalValue;
    
    public bool Initialized { get; internal set; } = Initialized;
    
    public Any Value {
        get => InternalValue;
        set {
            if (Field.IsInitOnly)
                throw new SpinorException("Cannot Set Value of Constant!");
            InternalValue = value;
        }
    }
}

public interface ITopModule {
    public SpinorContext Context { get; }
}

public abstract class Module : IAny<Module> {
    public readonly Symbol Name;
    public Module This => this;
    public Module Parent { get; protected init; }
    public abstract Type UnderlyingType { get; }
    public void Print(TextWriter tw) => tw.Write(Name);
    public object Lock => this;
    public virtual SpinorContext Context => Parent.Context;
    public static SType RuntimeType { get; set; }
    public abstract bool IsUsing(Module m);
    public abstract Any this[Symbol name] { get; set; }

    protected Module(Symbol name, Module parent) {
        if (parent != null) {
            Parent = parent;
        }else if (this is ITopModule) {
            Parent = this;
        }
        else throw new SpinorException("Cannot Initialize Module with No Parent!");
        Name = name;
    }
}

public class RuntimeModule : Module {
    public const TypeAttributes RuntimeModuleAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed;

    public readonly Dictionary<Symbol, Binding> Names = new();
    public readonly HashSet<Module> Usings = new();
    public ILTypeBuilder ModuleTypeBuilder { get; private set; }
    private Type _underlyingType;
    public override Type UnderlyingType => _underlyingType;
    public ModuleBuilder ModuleBuilder => ((SpinorRuntimeContext) Parent.Context).ModuleBuilder;
    public readonly string LongName;

    protected RuntimeModule(Symbol name, RuntimeModule parent, SpinorRuntimeContext ctx) : base(name, parent) {
        LongName = parent == null ? "Root." + name.String : parent.GetFullName(name);
        var tb = ctx.ModuleBuilder.DefineType(LongName, RuntimeModuleAttributes);
        ModuleTypeBuilder = new(tb);
        _underlyingType = tb;
    }

    public override Any this[Symbol name] {
        get => Names[name].Value;
        set {
            if (Names.TryGetValue(name, out var v))
                v.Value = value;
            else 
                SetGlobal(name, value);
        }
    }
    
    public override bool IsUsing(Module m) => Usings.Contains(m);

    public void SetConst<T>(Symbol name, T a) where T: Any {
        if (Names.ContainsKey(name))
            throw new SpinorException("Cannot rewrite constant {0}", name);
        
        var b = CreateBinding(name, typeof(T), true, a);
        
        ModuleTypeBuilder.TypeInitializer.Store.Field(b.Field);
    }

    public void SetGlobal(Symbol name, Any v) {
        var b = CreateBinding(name, typeof(Any), false);
        b.Value = v;
    }
    
    public Binding CreateBinding(Symbol name, SType t, bool isConst) => CreateBinding(name, t.UnderlyingType, isConst);

    public Binding CreateBinding(Symbol name, Type t, bool isConst, Any constValue = null) {
        Attributes a = Attributes.Public | Attributes.Static;
        if (isConst)
            a |= Attributes.Constant;
        
        Binding b = new(ModuleTypeBuilder.CreateField(name.String, t, a), this, false, constValue);
        Names.Add(name, b);
        return b;
    }

    public AbstractType NewAbstractType(Symbol name, AbstractType super, BuiltinType builtinType = BuiltinType.None) => AbstractType.Create(name, super, this, builtinType);
    public PrimitiveType NewPrimitiveType(Symbol name, AbstractType super, int bytelength) => PrimitiveType.Create(name, super, this, bytelength);

    public void Initialize() {
        if (_underlyingType is TypeBuilder)
           _underlyingType = ModuleTypeBuilder.Create();
        ModuleTypeBuilder = default;
    }

    public string GetFullName(Symbol name) => LongName + "." + name.String;
}

public sealed class RuntimeTopModule : RuntimeModule, ITopModule{
    public override SpinorRuntimeContext Context { get; }

    public RuntimeTopModule(Symbol name, SpinorRuntimeContext runtimeContext) : base(name, null, runtimeContext) {
        Context = runtimeContext;
    }
}

public class CompiledModule : Module {
    public override Type UnderlyingType { get; }
    public readonly ImmutableDictionary<Symbol, Binding> Names;
    public readonly ImmutableHashSet<Module> Usings;

    public CompiledModule(Symbol name, Module parent, Type underlyingType) : base(name, parent){
        Names = SpinorOptions.ReflectionEnabled ? 
                ImmutableDictionary.CreateRange(underlyingType.GetFields().
                        Select(x => new KeyValuePair<Symbol, Binding>((Symbol) x.Name, new(x, this, true)))) 
            : null;
        
        UnderlyingType = underlyingType;
        Usings = ImmutableHashSet<Module>.Empty;
    }
    
    public override bool IsUsing(Module m) => Usings.Contains(m);

    public override Any this[Symbol name] {
        get => Names[name].Value;
        set {
            var n = Names[name];
            n.Value = value;
        }
    }
}

public class CompiledTopModule : CompiledModule, ITopModule {
    public override SpinorCompiledContext Context { get; }

    public CompiledTopModule(Symbol name, SpinorCompiledContext context, Type underlyingType) : base(name, null, underlyingType) {
        Context = context;
    }
}