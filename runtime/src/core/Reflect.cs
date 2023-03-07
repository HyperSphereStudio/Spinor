/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Reflection;
using Core;
using runtime.core.memory;

namespace runtime.core;

public static class Reflect
{
    public static readonly MethodInfo
        GetRuntimeTypeMi = typeof(Type).GetMethod("GetTypeFromHandle"),
        RuntimeSTypeMi = typeof(IAny).GetMethod("get_RuntimeType"),
        GetTypeSTypeMI = typeof(Any).GetMethod("get_Type"),
        ObjectToStringMI = typeof(object).GetMethod("ToString"),
        ObjectGetHashCodeMI = typeof(object).GetMethod("GetHashCode"),
        StructToStringMI = typeof(ValueType).GetMethod("ToString"),
        StructGetHashCodeMI = typeof(ValueType).GetMethod("GetHashCode"),
        SerializeMI = typeof(Any).GetMethod("Serialize", BindingFlags.NonPublic | BindingFlags.Instance),
        SerializedWriteMI = typeof(SpinorSerializer).GetMethod("Write");
}