/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Runtime.CompilerServices;

namespace runtime.debug;

[Flags]
public enum DebugType : byte{
    JIL = 1,
    Runtime = 2,
    Dump = 4,
    
    JILDump = JIL | Dump,
    All = 0b11111111
}

public static class Debugger
{
    public const DebugType DebugLevel = DebugType.All;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Debug(DebugType ty, string format, params object[] parameters) {
        #if DEBUG2CONSOLE
            if(((int) DebugLevel & (int) ty) > 0)
                Console.Write(format, parameters);
        #endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Debugln(DebugType ty, string format, params object[] parameters) {
        Debug(ty, format, parameters);
        Debug(ty, "\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Debug(this string format, DebugType ty, params object[] parameters) =>
        Debug(ty, format, parameters);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Debugln(this string format, DebugType ty, params object[] parameters) =>
        Debugln(ty, format, parameters);
}