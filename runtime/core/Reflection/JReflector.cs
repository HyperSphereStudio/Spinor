using System;
using System.Reflection;
using runtime.core.Abstract;

namespace runtime.core.Reflection
{
    public class JReflector
    {
        public static bool IsJuliaType(Type t) => t.GetCustomAttribute<JuliaTypeAttribute>() != null;
    }
}