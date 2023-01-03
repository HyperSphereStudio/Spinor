using System;
using Core;
using runtime.core;
using runtime.core.Compilation;
using runtime.parse;

namespace runtime.core;

/**
   * @author : Johnathan Bizzano
   * @created : Monday, January 2, 2023
   * @modified : Monday, January 2, 2023
**/
public static class Init {
    private static void InitMainModule() {
        Modules.Main = new TopModule(CommonSymbols.Main, null);
        Modules.Main.Parent = Modules.Main;
        Modules.Main.SetConst(CommonSymbols.Core, Modules.Core);
        Modules.Core.SetConst(CommonSymbols.Main, Modules.Main);
    }

    internal static void Initialize() {
        Modules.Core = new TopModule(CommonSymbols.Core, null, false);
        Modules.Core.Parent = Modules.Core;
        Modules.TopModule = Modules.Core;
        Types.Init();
        SystemType.InitPrimitiveSystemTypes();
        Modules.Core.SetConst((Symbol) "Int", Environment.Is64BitOperatingSystem ? Types.Int64 : Types.Int32);
        InitMainModule();
        SpinorOperator.InitializeOperators();
    }
}