using System;
using System.Runtime.CompilerServices;

namespace runtime.Utils
{
    public static class EnumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static long ConvertToInt64<T>(T value) => Unsafe.As<T, long>(ref value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static T ConvertFromInt64<T>(long value) => Unsafe.As<long, T>(ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Include<T>(this T value, T append) where T : Enum =>
            ConvertFromInt64<T>(ConvertToInt64(value) | ConvertToInt64(append));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remove<T>(this T value, T rem) where T : Enum =>
            ConvertFromInt64<T>(ConvertToInt64(value) & ~ConvertToInt64(rem));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Set<T>(this T value, T flagSet, bool v = true) where T : Enum {
            if (v) return Include(value, flagSet);
            return Remove(value, flagSet);
        }
        
        
    }
}