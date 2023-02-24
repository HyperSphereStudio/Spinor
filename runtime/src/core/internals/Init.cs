using Core;
using runtime.core.CLR;
using runtime.core.Compilation;
using runtime.core.type;

using static runtime.core.Spinor;

namespace runtime.core;

/**
   * @author : Johnathan Bizzano
   * @created : Monday, January 2, 2023
   * @modified : Monday, January 2, 2023
**/
public static class Init {
    private static void InitMainModule(SpinorRuntimeContext ctx) {
        Main = new RuntimeTopModule(CommonSymbols.Main, ctx);
        Root = Main;
    }
    
    private static void BootstrapInitialization(SpinorRuntimeContext ctx) {
        Root = new RuntimeTopModule(CommonSymbols.Core, ctx);
        Spinor.Core = Root;
        
        Any.RuntimeType = new AbstractType(CommonSymbols.Any, null, Root, typeof(IAny), BuiltinType.None);
        var any = Any.RuntimeType;
        any.Super = any;

        SType.CreateTypeFromSystem(typeof(Symbol));
        SType.CreateTypeFromSystem(typeof(Module));
        SType.CreateTypeFromSystem(typeof(Expr));
        SType.CreateTypeFromSystem(typeof(LineNumberNode));

        SType.RuntimeType = Root.NewAbstractType(CommonSymbols.Type, any);
        Signed = Root.NewAbstractType(CommonSymbols.Signed, null, BuiltinType.SignedInteger);
        AbstractFloat = Root.NewAbstractType(CommonSymbols.AbstractFloat, null, BuiltinType.FloatingNumber);
        
        Int64 = (NumericType) Root.NewPrimitiveType((Symbol) "Int64", Signed, 8);
        Bool = (NumericType) Root.NewPrimitiveType((Symbol) "Bool", Signed, 1);
        Float64 = (NumericType) Root.NewPrimitiveType((Symbol) "Float64", AbstractFloat, 8);

        SType.RuntimeType.Initialize();
        Signed.Initialize();
        AbstractFloat.Initialize();
        BoxedPrimitiveType<long>.Create(Int64);
        BoxedPrimitiveType<double>.Create(Float64);
        BoxedPrimitiveType<bool>.Create(Bool);
        
        Root.Initialize();
        Main.Initialize();
        ctx.Save("Runtime.dll");
        // AbstractString = Modules.Root.NewAbstractType((Symbol) "AbstractString", any);
        // Function = Modules.Root.NewAbstractType((Symbol) "Function", any);
    }

    internal static void Initialize() {
        var sc = new ClrSpinorRuntimeContext();
        InitMainModule(sc);
        BootstrapInitialization(sc);
        SpinorOperator.InitializeOperators();
    }
}