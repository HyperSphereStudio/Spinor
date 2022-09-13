using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace runtime.core {
    public class IO {

        public static void STOUTPrint(object o) => Console.Write(o);
        public static void STOUTPrintLn(object o) => Console.WriteLine(o);

        public static void PrintLn(params object[] v) {
            foreach(var t in v)
                Console.Write(t);
            Console.WriteLine();
        }
        
        public static void Print(params object[] v) {
            foreach(var t in v)
                Console.Write(t);
        }

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
    }
}