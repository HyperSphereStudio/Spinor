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
using runtime.core.compilation.Expr;
using runtime.core.type;
using runtime.ILCompiler;

namespace Core;
using static FieldAttributes;

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

public abstract class Module : IAny {
    public readonly Symbol Name;
    public Module Parent { get; protected init; }
    public abstract Type UnderlyingType { get; }
    public void Print(TextWriter tw) => tw.Write(Name);
    public object Lock => this;
    public virtual SpinorContext Context => Parent.Context;
    public static SType RuntimeType { get; set; }
    public SType Type => RuntimeType;
    public abstract bool IsUsing(Module m);
    public abstract Any this[Symbol name] { get; set; }
    public override string ToString() => Name.String;

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

public class CompileTimeModule : Module {
    public readonly Dictionary<Symbol, Binding> Names = new();
    public readonly HashSet<Module> Usings = new();
    public ILTypeBuilder ModuleTypeBuilder { get; private set; }
    private Type _underlyingType;
    public override Type UnderlyingType => _underlyingType;
    public ModuleBuilder ModuleBuilder => ((SpinorRuntimeContext) Parent.Context).ModuleBuilder;
    public readonly string LongName;

    protected CompileTimeModule(Symbol name, CompileTimeModule parent, SpinorRuntimeContext ctx) : base(name, parent) {
        LongName = parent == null ? "Root." + name.String : parent.GetFullName(name);
        var tb = ctx.ModuleBuilder.DefineType(LongName, TypeAttributes.Public | TypeAttributes.Sealed  | TypeAttributes.Abstract | TypeAttributes.Class);
        ModuleTypeBuilder = new(tb);
        _underlyingType = tb;
    }

    public bool HasName(Symbol name) => Names.ContainsKey(name);

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

    public void SetConst<T>(Symbol name, T a) where T: IAny {
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
        var a = Public | Static;
        if (isConst)
            a |= InitOnly;
        
        Binding b = new(ModuleTypeBuilder.CreateField(name.String, t, a), this, false, constValue);
        Names.Add(name, b);
        return b;
    }

    public AbstractType NewAbstractType(Symbol name, AbstractType super, BuiltinType builtinType = BuiltinType.None) => AbstractType.Create(name, super, this, builtinType);
    public PrimitiveType NewPrimitiveType(Symbol name, AbstractType super, int bytelength) => PrimitiveType.Create(name, super, this, bytelength);
    public CompileTimeModule NewModule(Symbol name, bool isBare) => new(name, this, (SpinorRuntimeContext) Context);
    public StructType NewStructType(StructKind kind, Symbol name, AbstractType super, params Field[] fields) => StructType.Create(kind, name, super, this, fields);
    
    public void Initialize() {
        if (_underlyingType is TypeBuilder)
           _underlyingType = ModuleTypeBuilder.Create();
        ModuleTypeBuilder = default;
    }

    public string GetFullName(Symbol name) => LongName + "." + name.String;
    public Any Evaluate(Any a) => new GlobalExprInterpreter(this).Evaluate(a);
}

public sealed class CompileTimeTopModule : CompileTimeModule, ITopModule{
    public override SpinorRuntimeContext Context { get; }

    public CompileTimeTopModule(Symbol name, SpinorRuntimeContext runtimeContext) : base(name, null, runtimeContext) {
        Context = runtimeContext;
    }
}

public class RuntimeModule : Module {
    public override Type UnderlyingType { get; }
    public readonly ImmutableDictionary<Symbol, Binding> Names;
    public readonly ImmutableHashSet<Module> Usings;

    public RuntimeModule(Symbol name, Module parent, Type underlyingType) : base(name, parent){
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

public class RuntimeTopModule : RuntimeModule, ITopModule {
    public override SpinorCompiledContext Context { get; }

    public RuntimeTopModule(Symbol name, SpinorCompiledContext context, Type underlyingType) : base(name, null, underlyingType) {
        Context = context;
    }
}