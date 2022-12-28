using System;
using System.Runtime.CompilerServices;

namespace runtime.Utils
{
    public static class EnumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static C ConvertTo<T, C>(this T value) where T: Enum => Unsafe.As<T, C>(ref value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T ConvertFrom<T, C>(this C value) where C: unmanaged => Unsafe.As<C, T>(ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Include<T>(this T value, T append) where T : Enum =>
            ConvertFrom<T, long>(ConvertTo<T, long>(value) | ConvertTo<T, long>(append));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remove<T>(this T value, T rem) where T : Enum =>
            ConvertFrom<T, long>(ConvertTo<T, long>(value) & ~ConvertTo<T, long>(rem));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Set<T>(this T value, T flagSet, bool v = true) where T : Enum {
            if (v) return Include(value, flagSet);
            return Remove(value, flagSet);
        }
        
        
    }
}