using System;
using Core;

namespace runtime.core;

public static class Init
{
    
    private static void InitPrimitives() {
        foreach (var t in new[] {
                     Types.Module, Types.Any, Types.Type,
                     Types.DataType, Types.Symbol, Types.Nothing,
                     Types.AbstractString, Types.Function, Types.Builtin
                 }) {
            Modules.Core.SetConst(t.Name, t);
        }

        Modules.Core.SetConst((Symbol) "Int", Environment.Is64BitOperatingSystem ? Types.Int64 : Types.Int32);
    }
    
    
    private static void InitMainModule() {
        Modules.Main = new Module(CommonSymbols.Main, null);
        Modules.Main.Parent = Modules.Main;
        Modules.Main.SetConst(CommonSymbols.Core, Modules.Core);
        Modules.Core.SetConst(CommonSymbols.Main, Modules.Main);
    }

    internal static void Initialize() {
        Types.Init();
        
        Modules.Core = new Module(CommonSymbols.Core, null, false);
        Modules.Core.Parent = Modules.Core;
        Modules.TopModule = Modules.Core;
        InitPrimitives();
        InitMainModule();
        
    }
}