﻿/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace runtime.core;

public static class IO {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* UnsafeWrite<T>(void* destination, T v) where T: unmanaged{
        Unsafe.Write(destination, v);
        return (byte*) destination + sizeof(T);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* UnsafeRead<T>(void* source, out T v) where T: unmanaged {
        v = *(T*) source;
        return (byte*) source + sizeof(T);
    }

    public static void Err(this object o) => Console.Error.Write(o);
    public static void ErrLn(this object o) => Console.Error.WriteLine(o);
    
    public static void Err(this string format, params object[] parameters) => Console.Error.Write(format, parameters);

    public static void ErrLn(this string format, params object[] parameters) =>
        Console.Error.WriteLine(format, parameters);
    
    public static void Print(this object o) => Console.Out.Write(o ?? "null");
    public static void PrintLn(this object o) => Console.Out.WriteLine(o ?? "null");
    
    public static void Print(this string format, params object[] parameters) => Console.Out.Write(format, parameters);

    public static void PrintLn(this string format, params object[] parameters) =>
        Console.Out.WriteLine(format, parameters);

    private static void Repeat(TextWriter tw, string s, int n) {
        for(int i = 0; i < n; i++)
            tw.Write(s);
    }
   
    
    
}