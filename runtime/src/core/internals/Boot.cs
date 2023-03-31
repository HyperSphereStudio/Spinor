/*
   * Author : Johnathan Bizzano
   * Created : Tuesday, March 14, 2023
   * Last Modified : Tuesday, March 14, 2023
*/

using System.Runtime.CompilerServices;
using Core;
using runtime.core.math;
using runtime.core.type;
using runtime.stdlib;
using static runtime.core.internals.Spinor;
using Module = System.Reflection.Module;

namespace runtime.core.internals;

public static class Boot {
    public static SpinorPhase ProgramPhase { get; internal set; } = SpinorPhase.Initializing;
    
    private static void InitMainModule() {
        ProgramPhase = SpinorPhase.Initializing;
        Main = CompileTimeTopModule.CreateModule(CommonSymbols.Main, "0.0.0.0");
        Root = Main;
    }
    
    public static void Launch() {
        try {
            if (ProgramPhase != SpinorPhase.Initializing)
                return;
            
            FromScratch();
        
            ProgramPhase = SpinorPhase.Running;
        }catch (SpinorException e) {
            e.Print();
            throw;
        }
    }
    
    private static void FromScratch() {
        ProgramPhase = SpinorPhase.Bootstrapping;
        Spinor.Core = CompileTimeTopModule.CreateModule(CommonSymbols.Core, "0.0.0.1");
        
        Any.RuntimeType = new AbstractType(CommonSymbols.Any, null, Root, 0, typeof(IAny));
        Any.RuntimeType.Super = (AbstractType) Any.RuntimeType;
        RuntimeHelpers.RunClassConstructor(typeof(SType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SystemValue<string>).TypeHandle);
        Root.SetConst(CommonSymbols.Any, Any.RuntimeType);
        
        Symbol.RuntimeType = Root.InitializeStructType(SpinorTypeAttributes.Class, "Symbol", Any.RuntimeType, 1, null, 
            new TypeLayout(0, typeof(Symbol), null, null, null));
        stdlib.Module.RuntimeType = Root.InitializeStructType(SpinorTypeAttributes.Class, "Module", Any.RuntimeType, 1, null, 
            new TypeLayout(0, typeof(stdlib.Module), null, null, null));
        Spinor.Nothing = System<Nothing>.Box(new Nothing());

        SpinorOperator.InitializeOperators();
        EvalFromFile("runtime/Core/Boot.jl");
        Root.Save();  
    }
}