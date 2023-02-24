/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Reflection;
using Core;

namespace runtime.core;

public static class Reflect
{
    public static readonly Type IAny_TY = typeof(Any).Module.GetType("Core.IAny`1");
    public static readonly Type IPrimitiveValue_TY = typeof(PrimitiveType).Module.GetType("Core.IPrimitiveValue`1");
    public static readonly MethodInfo
        GET_RUNTIME_TYPE_MI = typeof(System.Type).GetMethod("GetTypeFromHandle");
}