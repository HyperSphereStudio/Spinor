/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Reflection;
using Core;
using runtime.core.compilation.ILCompiler;
using runtime.core.internals;
using runtime.core.memory;
using runtime.core.type;
using runtime.stdlib;
using Module = runtime.stdlib.Module;

namespace runtime.core;
using static BindingFlags;

public static class Reflect
{
    public static readonly MethodInfo
        Type_GetRuntimeType = typeof(Type).GetMethod("GetTypeFromHandle"),
        GC_AllocateUnitialized_1 = typeof(GC).GetMethod("AllocateUninitializedArray"),

        Object_ToString = typeof(object).GetMethod("ToString"),
        Object_GetHashCode = typeof(object).GetMethod("GetHashCode"),
        Object_Equals = typeof(object).GetMethod("Equals", Type.EmptyTypes),

        SerializeMI = typeof(IAny).GetMethod("Serialize", NonPublic | Instance),
        SerializedWriteMI = typeof(SpinorSerializer).GetMethod("Write"),

        IAny_GetType = typeof(IAny).GetMethod("GetType"),
        IAny_GetRuntimeType = typeof(IAny).GetMethod("get_RuntimeType"),
        Any_GetRuntimeType = typeof(Any).GetMethod("get_RuntimeType"),

        Any_SystemBox1 = typeof(Any).GetMethod("SystemBox`1"),
        Any_Box1 = typeof(Any).GetMethod("Box`1"),
        
        Module_InitializeAbstractType = typeof(Module).GetMethod("InitializeAbstractType"),
        Module_InitializeSystemType = typeof(Module).GetMethod("InitializeSystemType"),
        Module_InitializeStructType = typeof(Module).GetMethod("InitializeStructType"),
        Module_InitializeModule = typeof(Module).GetMethod("InitializeModule"),
        Module__ToRootModule = typeof(Module).GetMethod("_ToRootModule", NonPublic|Static),

        Symbol_ToSymbol = typeof(Symbol).GetCastOperator(typeof(string), true, true),
        
        Array_Empty = typeof(Array).GetMethod("Empty");

    public static readonly PropertyInfo
        ISystemAny_Value = typeof(ISystemAny).GetProperty("Value");

    public static readonly ConstructorInfo 
        Any_New_ISystemAny = typeof(Any).GetConstructor(new[]{typeof(ISystemAny)}),
        Any_New_ISpinorAny = typeof(Any).GetConstructor(new[]{typeof(ISpinorAny)}),
        RuntimeTopModule = typeof(RuntimeTopModule).GetConstructors()[0],
        AbstractType = typeof(AbstractType).GetConstructors()[0],
        StructType = typeof(StructType).GetConstructors()[0],
        SpinorField_New = typeof(SpinorField).GetConstructor(new[]{typeof(string), typeof(RuntimeFieldHandle), typeof(SType), typeof(SpinorFieldAttributes)}),
        TypeLayout_New = typeof(TypeLayout).GetConstructor(new[]{typeof(int), typeof(Type), typeof(SpinorField[]), typeof(SpinorProperty[]), typeof(SpinorParameter[])});

    public static readonly FieldInfo 
        Any_Value = typeof(Any).GetField("Value");

    public static readonly Type[] ObjectType = { typeof(object) };
    
}