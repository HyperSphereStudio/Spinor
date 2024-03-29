/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using runtime.core;

namespace runtime.Utils;

public static class ListExtensions
{
    public static void Put<K, V>(this IDictionary<K, V> d, K k, V v) {
        if (d.ContainsKey(k))
            d[k] = v;
        d.Add(k, v);
    }
    
    public static bool Visit<T>(this IEnumerable<T> e, Func<T, bool> v) {
        foreach (var t in e)
            if (!v(t))
                return false;
        return true;
    }
    
    public static bool Visit<T>(this IEnumerable<T> e, Func<T, int, bool> v) {
        int i = 0;
        foreach (var t in e)
            if (!v(t, i++))
                return false;
        return true;
    }
    
    public static TextWriter Print<T>(this IEnumerable<T> l, TextWriter tw = null, Func<T, string> toStringMethod = null, int depth = 3) {
        if (depth-- == 0)
            return tw;
        var isFirst = true;
        toStringMethod ??= x => x.ToString();
        tw ??= Console.Out;
        
        tw.Write(typeof(T).Name);
        tw.Write("[");
        
        foreach (var x in l) {
            if (isFirst)
                isFirst = false;
            else 
                tw.Write(", ");
            tw.Write(toStringMethod(x));
        }
        
        tw.Write("]");
        return tw;
    }

    public static TextWriter Print(this IEnumerable l, Type elType, Func<object, string> toStringMethod = null, TextWriter tw = null, int depth = 3) {
        if (depth-- == 0)
            return tw;
        var isFirst = true;
        toStringMethod ??= x => x.ToString();
        tw ??= Console.Out;
        
        tw.Write(elType.Name);
        tw.Write("[");
        foreach (var x in l) {
            if (isFirst)
                isFirst = false;
            else 
                tw.Write(", ");
            tw.Write(toStringMethod(x));
        }
        
        tw.Write("]");
        return tw;
    }

    public static TextWriter Print(this IEnumerable l, TextWriter tw = null, Func<object, string> toStringMethod = null, int depth = 3) => Print(l, l.GetType().GetElementType(), toStringMethod, tw, depth);
    public static void PrintLn<T>(this IEnumerable<T> l, TextWriter tw = null, Func<T, string> toStringMethod = null, int depth = 3) => Print(l, tw, toStringMethod, depth).WriteLine();
    public static void PrintLn(this IEnumerable l, TextWriter tw = null, Func<object, string> toStringMethod = null, int depth = 3) => Print(l, tw, toStringMethod, depth).WriteLine();
}