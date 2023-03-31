/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using runtime.core;
using runtime.core.compilation.ILCompiler;
using runtime.core.compilation.interp;
using runtime.core.internals;
using runtime.core.type;
using runtime.core.Utils;
using runtime.ILCompiler;

namespace runtime.stdlib;
using static FieldAttributes;
using static TypeAttributes;

//Field used for internal building
public record struct Binding(FieldInfo Field, Module Module, Any InternalValue = default) {
    //Runtime Value
    private Any InternalValue { get; set; } = InternalValue;

    public Any Value {
        get => InternalValue;
        set {
            if (Field.IsInitOnly)
                throw new SpinorException("Cannot Set Value of Constant!");
            InternalValue = value;
        }
    }
}

public abstract class Module : ISpinorAny
{
    public readonly Symbol Name;
    public Module Parent { get; }
    public void Print(TextWriter tw) => tw.Write(Name);
    public virtual Module TopModule => Parent.TopModule;
    public static SType RuntimeType { get; internal set; }
    public SType Type => RuntimeType;
    public virtual FieldInfo ModuleField => GetBinding(Name).Field;
    public Any this[Symbol name] => TryGetName(name, out var v) ? v : default;
    public override string ToString() => Name.String;
    void IAny.Print(TextWriter tw) => tw.Write(Name.String);
    public bool TryGetName(Symbol name, out Any v) {
        if (TryGetBinding(name, out var b)) {
            v = b.Value;
            return true;
        }
        v = default;
        return false;
    }
    public abstract bool TryGetBinding(Symbol name, out Binding b);
    public abstract bool TryGetSystemName(Symbol name, out Type t);

    public Binding GetBinding(Symbol name) {
        if (!TryGetBinding(name, out var b))
            throw new SpinorException($"Name {name} does not exist in {Name}");
        return b;
    }

    protected Module(Symbol name, Module parent) {
        if (parent != null) {
            Parent = parent;
        }else if (this is ITopModule)
            Parent = this;
        else throw new SpinorException("Cannot Initialize Module with No Parent!");
        Name = name;
    }

    public static bool FindRootModule(Symbol name, out Module m) {
        if (Spinor.TopModules.TryGetValue(name, out var t)) {
            m = t;
            return true;
        }
        m = default;
        return false;
    }

    private static Module _ToRootModule(string name, Type callingType, Module[] usings, string[] systemUsings) {
        if (!callingType.Assembly.IsDynamic)
            return new RuntimeTopModule((Symbol) name, callingType, usings, systemUsings);
        if (!FindRootModule((Symbol) name, out var m))
            throw new SpinorException($"Unable To Find Module {name}");
        return m;
    }
    
    public static implicit operator Any(Module m) => new(m);
    public static implicit operator Module(Any a) => a.Cast<Module>();

    public virtual StructType InitializeStructType(SpinorTypeAttributes attributes, string name, SType super, int specif, RuntimeMethodHandle[] ctors, TypeLayout layout) => new ((Symbol)name, super, this, attributes, specif, ctors, layout);
    public virtual AbstractType InitializeAbstractType(string name, SType super, int specif, Type t) => new ((Symbol) name, super, this, (ushort) specif, t);
    public virtual Module InitializeModule(string name, Type t, Module[] usings, string[] systemUsings) => 
        new RuntimeModule((Symbol) name, this, t, usings, systemUsings);
}

public class CompileTimeModule : Module {
    public readonly Dictionary<Symbol, Binding> Names = new();
    public readonly HashSet<string> SystemUsings = new();
    public readonly HashSet<Module> Usings = new();
    public readonly TypeBuilder ModuleBuilder;
    public readonly string LongName;
    public readonly IlExprBuilder ModuleInitializer;

    protected CompileTimeModule(Symbol name, CompileTimeModule parent, ModuleBuilder mb) : base(name, parent) {
        if (this is CompileTimeTopModule c) {
            LongName = "Root." + name.String;
            Spinor.Root = c;
        }else
            LongName = parent.GetFullName(name);
        LongName = string.Intern(LongName);

        ModuleBuilder = mb.DefineType(LongName, TypeAttributes.Public|Sealed|Abstract|Class);
        ModuleInitializer = ModuleBuilder.DefineTypeInitializer();
        CreateBinding(name, typeof(Module), true, this);

        if (this is not CompileTimeTopModule)
            WriteSetModuleField();
    }

    private void WriteSetModuleField() {
        ModuleInitializer.Load.Field(Parent.ModuleField);
        ModuleInitializer.Load.String(Name.String);
        ModuleInitializer.Load.Type(ModuleBuilder);
        ModuleInitializer.Function.Invoke(Reflect.Module_InitializeModule);
        ModuleInitializer.Store.Field(ModuleField);
    }

    public new Any this[Symbol name] {
        get => base[name];
        set {
            if (Names.TryGetValue(name, out var v))
                v.Value = value;
            else 
                SetGlobal(name, value);
        }
    }

    public override bool TryGetBinding(Symbol name, out Binding b) {
        if (Names.TryGetValue(name, out b))
            return true;

        foreach(var n in Usings)
            if (n.TryGetBinding(name, out b))
                return true;
        
        b = default;
        return false;
    }
    
    public override bool TryGetSystemName(Symbol name, out Type t) {
        foreach (var st in SystemUsings) {
            if ((t = System.Type.GetType(st + "." + name, false)) != null)
                return true;
        }

        if ((t = System.Type.GetType(name.String, false)) != null)
            return true;
        
        t = default;
        return false;
    }

    public void SetConst(Symbol name, Any v) {
        if (Names.ContainsKey(name))
            throw new SpinorException("Cannot rewrite constant {0}", name);
        var b = CreateBinding(name, v.Type, true, v);

        switch (v.Value) {
            case SType s:
                ModuleInitializer.Function.Invoke(s.GetRuntimeTypeMethod);
                break;
            case Module m:
                ModuleInitializer.Load.Field(m.ModuleField);
                break;
            default:
                throw new NotImplementedException();
        }
        
        ModuleInitializer.Store.Field(b.Field); 
    }

    public void SetGlobal(Symbol name, Any v) {
        var b = CreateBinding(name, Any.RuntimeType, false);
        b.Value = v;
    }

    public Binding CreateBinding(Symbol name, SType t, bool isConst, Any constValue = default) => CreateBinding(name, t.UnderlyingType, isConst, constValue);
    public Binding CreateBinding(Symbol name, Type t, bool isConst, Any constValue = default) {
        var a = FieldAttributes.Public | Static;
        if (isConst)
            a |= InitOnly;
        Binding b = new(ModuleBuilder.CreateField(name.String, t, a), this, constValue);
        Names.Add(name, b);
        return b;
    }

    public Module Using(Module m) {
        Usings.Add(m);
        return m;
    }

    public void UsingSystem(string name) => SystemUsings.Add(name);
    public AbstractTypeBuilder NewAbstractType(Symbol name, AbstractType super) => new(){Name=name, Super=super, Module=this};
    public StructTypeBuilder NewStructType(Symbol name, AbstractType super, SpinorTypeAttributes attributes) => new (){Attributes=attributes, Name=name, Super=super, Module=this};
    public CompileTimeModule NewModule(Symbol name, bool isBare) => new(name, this, Spinor.Root.ModuleScope);

    public virtual void Initialize() {
        if (ModuleBuilder.IsCreated())
            return;
        
        ModuleInitializer.ReturnVoid();
        ModuleBuilder.CreateType();
    }

    public string GetFullName(Symbol name) => LongName + "." + name.String;
    public Any Evaluate(Any a) => new GlobalExprInterpreter(this).Evaluate(a);

    public override Module InitializeModule(string name, Type t, Module[] usings, string[] systemUsings) {
        var sname = (Symbol) name;
        if (TryGetName(sname, out var v)) 
            return v;
        v = base.InitializeModule(name, t, usings, systemUsings);
        SetConst(sname, v);
        return v;
    }

    public override StructType InitializeStructType(SpinorTypeAttributes attributes, string name, SType super,
        int specif, RuntimeMethodHandle[] ctors, TypeLayout layout) {
        var sname = (Symbol) name;
        if (TryGetName(sname, out var v)) 
            return (StructType) v;
        v = base.InitializeStructType(attributes, name, super, specif, ctors, layout);
        SetConst(sname, v);
        return (StructType) v;
    }

    public override AbstractType InitializeAbstractType(string name, SType super, int specif, Type t) {
        var sname = (Symbol) name;
        if (TryGetName(sname, out var v)) 
            return (AbstractType) v;
        v = base.InitializeAbstractType(name, super, specif, t);
        SetConst(sname, v);
        return (AbstractType) v;
    }
}

public interface ITopModule {}

public sealed class CompileTimeTopModule : CompileTimeModule, ITopModule{
    public ModuleBuilder ModuleScope { get; }
    public override FieldInfo ModuleField { get; }
    public readonly ILTypeBuilder GlobalConstants;
    public override Module TopModule => this;
    
    private CompileTimeTopModule(Symbol name, ModuleBuilder mb) : base(name, null, mb) {
        ModuleScope = mb;
        Spinor.TopModules.Add(name, this);

        //Needed for highly used interned global variables
        GlobalConstants = new(ModuleScope.DefineType("Root.Constants", TypeAttributes.Public|Sealed|Class|Abstract));
        
        WriteHelper(out var rm);
        ModuleField = rm;

        //Set the internal module field to the root module field from helper
        ModuleInitializer.Load.Field(rm);
        ModuleInitializer.Store.Field(GetBinding(Name).Field);
    }
    
    
    private void WriteHelper(out FieldInfo topModule) {
        ILTypeBuilder helperType = new(ModuleScope.DefineType("Root.Helper", TypeAttributes.Public|Abstract|Sealed));
        topModule = WriteGetRootMethod(helperType);
        helperType.Create();
    }

    private FieldInfo WriteGetRootMethod(ILTypeBuilder helperType) {
        var ex = helperType.TypeInitializer;
        var topModule = helperType.CreateField("Root", typeof(Module), FieldAttributes.Public|Static|InitOnly);

        var usingsArray = ex.Array.Serialize1D<Module, Module>(Usings, Usings.Count, m => ex.Load.Field(m.ModuleField));
        var systemUsingsArray = ex.Array.Serialize1D<string, string>(SystemUsings, SystemUsings.Count, s => ex.Load.String(s));

        ex.Load
            .String(Name.String)
            .Type(ModuleBuilder)
            .Local(usingsArray)
            .Local(systemUsingsArray);
      
        ex.Function.Invoke(Reflect.Module__ToRootModule);
        ex.Store.Field(topModule);

        return topModule;
    }

    internal static CompileTimeTopModule CreateModule(Symbol name, string version) =>
        new(name, AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName($"{name}, Version={version}"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("SPINOR_RUNTIME_MODULE"));

    public override void Initialize() {
        GlobalConstants.TypeInitializer.ReturnVoid();
        GlobalConstants.Create();
        base.Initialize();
    }
    
    public void Save() {
        Spinor.Root.Initialize();
        var asm = new Lokad.ILPack.AssemblyGenerator();
        var name = ModuleScope.Assembly.GetName().Name;
        asm.GenerateAssembly(ModuleScope.Assembly, new[]{ModuleScope.Assembly}, $"{name}.dll");
        Console.WriteLine($"Generated Assembly {name}.dll");
    }
}

public class RuntimeModule : Module {
    public readonly Module[] Usings;
    public readonly string[] SystemUsings;
    public readonly Type ModuleType;

    public RuntimeModule(Symbol name, Module parent, Type underlyingType, Module[] usings, string[] systemUsings) : base(name, parent){
        Usings = usings;
        SystemUsings = systemUsings;
        ModuleType = underlyingType;
    }

    public new Any this[Symbol name] {
        set {
            if (TryGetBinding(name, out var b))
                b.Field.SetValue(null, value);
        }
    }

    public override bool TryGetBinding(Symbol name, out Binding b) {
        var fi = ModuleType.GetField(name.String);
        if (fi != null) {
            b = new(fi, this);
            return true;
        }
        b = default;
        return false;
    }

    public override bool TryGetSystemName(Symbol name, out Type t) {
        foreach (var st in SystemUsings) {
            if ((t = System.Type.GetType(st + "." + name, false)) != null)
                return true;
        }
        t = default;
        return false;
    }
}

public class RuntimeTopModule : RuntimeModule, ITopModule {
    public override FieldInfo ModuleField { get; }
    public override Module TopModule => this;

    public RuntimeTopModule(Symbol name, Type underlyingType, Module[] usings, string[] systemUsings) : 
        base(name, null, underlyingType, usings, systemUsings) {
        Spinor.TopModules.Add(name, this);
        var helperType = underlyingType.Assembly.GetType("Root.Helper");
        ModuleField = helperType.GetField("Root");
    }
}

