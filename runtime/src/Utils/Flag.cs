/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Runtime.CompilerServices;

namespace runtime.Utils
{
    public static class EnumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static C ConvertTo<T, C>(this T value) where T: Enum => Unsafe.As<T, C>(ref value);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static int ToInt<T>(this T value) where T: Enum => Unsafe.As<T, int>(ref value);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static T ConvertFrom<T, C>(this C value) where C: unmanaged => Unsafe.As<C, T>(ref value);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static T Include<T>(this T value, T append) where T : Enum => ConvertFrom<T, int>(value.ToInt() | append.ToInt());
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static T Remove<T>(this T value, T rem) where T : Enum => ConvertFrom<T, int>(value.ToInt() & ~rem.ToInt());
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static T Set<T>(this T value, T flagSet, bool v = true) where T : Enum => v ? Include(value, flagSet) : Remove(value, flagSet);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] public static bool MaskIs<T>(this T m, T attrib, T mask) where T : Enum => (m.ToInt() & mask.ToInt()) == attrib.ToInt();
    }
}