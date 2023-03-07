/*
   * Author : Johnathan Bizzano
   * Created : Thursday, March 2, 2023
   * Last Modified : Thursday, March 2, 2023
*/

using System;
using System.Linq;
using System.Reflection;

namespace runtime.Utils;

public static class Utils
{
   public static bool IsUnmanaged(this Type type) {
      if (type.IsPrimitive || type.IsPointer || type.IsEnum)
         return true;
        
      if (!type.IsValueType)
         return false;
        
      return type
         .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
         .All(f => IsUnmanaged(f.FieldType));
   }
}