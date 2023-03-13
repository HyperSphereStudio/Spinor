using System;
using System.Reflection;
using Core;
using runtime.core.CLR;
using runtime.core.Compilation;
using runtime.core.type;

using static runtime.core.Spinor;
using Module = Core.Module;

namespace runtime.core;

/**
   * @author : Johnathan Bizzano
   * @created : Monday, January 2, 2023
   * @modified : Monday, January 2, 2023
**/
public static class Init {
    private static void InitMainModule(SpinorRuntimeContext ctx) {
        ProgramPhase = SpinorPhase.Initializing;
        Main = new CompileTimeTopModule(CommonSymbols.Main, ctx);
        Root = Main;
        
    }
    
    private static void BootstrapInitialization() {
        ProgramPhase = SpinorPhase.BootstrappingReal;
        
        var ctx = new ClrSpinorRuntimeContext("Core", "0.0.0.1");
        Root = new CompileTimeTopModule(CommonSymbols.Core, ctx);
        Spinor.Core = Root;
        
        Any.RuntimeType = new AbstractType(CommonSymbols.Any, null, Root, typeof(IAny), BuiltinType.None);
        var any = Any.RuntimeType;
        any.Super = any;

        SType.CreateTypeFromSystem(typeof(Symbol));
        SType.CreateTypeFromSystem(typeof(Module));
        SType.CreateTypeFromSystem(typeof(Expr));
        SType.CreateTypeFromSystem(typeof(LineNumberNode));

        ProgramPhase = SpinorPhase.BootstrappingFake;
        SType.RuntimeType = Root.NewAbstractType(CommonSymbols.Type, any);
        Signed = Root.NewAbstractType((Symbol) "__boot__Signed", null, BuiltinType.SignedInteger);
        AbstractFloat = Root.NewAbstractType((Symbol) "__boot__AbstractFloat", null, BuiltinType.FloatingNumber);
        Spinor.Int64 = (NumericType) Root.NewPrimitiveType((Symbol) "__boot__Int64", Signed, 8);
        Bool = (NumericType) Root.NewPrimitiveType((Symbol) "__boot__Bool", Signed, 1);
        Float64 = (NumericType) Root.NewPrimitiveType((Symbol) "__boot__Float64", AbstractFloat, 8);
        
        BoxedPrimitiveType<long>.Create(Spinor.Int64);
        BoxedPrimitiveType<double>.Create(Float64);
        BoxedPrimitiveType<bool>.Create(Bool);
        
        ProgramPhase = SpinorPhase.BootstrappingReal;
        ((Expr) ParseFile("runtime/Core/Boot.jl")).WriteCode(Console.Out);
        EvalFromFile("runtime/Core/Boot.jl");

        //Load the Primitive Types from Boot then Create boxing functions foreach of them
        foreach (var ty in Root.Names) {
            if (ty.Value.Value is not NumericType nty) continue;
            var boxedType = typeof(BoxedPrimitiveType<>).MakeGenericType(nty.ValueField.FieldType);
            boxedType.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[]{nty});
        }
        
        //Map __boot__ -> real types
        Signed = (AbstractType) Root[CommonSymbols.Signed];
        Unsigned = (AbstractType) Root[CommonSymbols.Unsigned];
        Spinor.Int64 = (NumericType) Root[(Symbol) "Int64"];
        Float64 = (NumericType) Root[(Symbol) "Float64"];
        Bool = (NumericType) Root[(Symbol) "Bool"];
        Spinor.Exception = (AbstractType) Root[(Symbol) "Exception"];
        
        Root.SetConst((Symbol) "Int", (NumericType) (ctx.PointerSize == 4 ? Root[(Symbol) "Int32"] : Spinor.Int64));
        Root.SetConst((Symbol) "UInt", (NumericType) (ctx.PointerSize == 4 ? Root[(Symbol) "UInt32"] : Root[(Symbol) "UInt64"]));
        Root.SetConst((Symbol) "Float", (NumericType) (ctx.PointerSize == 4 ? Root[(Symbol) "Float32"] : Float64));
        Root.SetConst(CommonSymbols.Any, Any.RuntimeType);

        ctx.Save();
    }

    internal static void Initialize() {
        if (ProgramPhase != SpinorPhase.Initializing)
            throw new SpinorException("Already Initialized!");

        SpinorOperator.InitializeOperators();
        BootstrapInitialization();
        
        ProgramPhase = SpinorPhase.Running;
    }
}