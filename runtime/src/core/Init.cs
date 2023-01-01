using System;
using Core;
using runtime.core.expr;
using runtime.parse;

namespace runtime.core;

public static class Init {
    private static void InitMainModule() {
        Modules.Main = new Module(CommonSymbols.Main, null);
        Modules.Main.Parent = Modules.Main;
        Modules.Main.SetConst(CommonSymbols.Core, Modules.Core);
        Modules.Core.SetConst(CommonSymbols.Main, Modules.Main);
    }

    internal static void Initialize() {
        Modules.Core = new Module(CommonSymbols.Core, null, false);
        Modules.Core.Parent = Modules.Core;
        Modules.TopModule = Modules.Core;
        Types.Init();
        SystemType.InitPrimitiveSystemTypes();
        Modules.Core.SetConst((Symbol) "Int", Environment.Is64BitOperatingSystem ? Types.Int64 : Types.Int32);
        InitMainModule();
        SpinorOperator.InitializeOperators();
    }
}