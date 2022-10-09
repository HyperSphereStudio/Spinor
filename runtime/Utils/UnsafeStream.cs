using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using runtime.core;

namespace runtime.Utils
{
    public unsafe class UnsafeStream
    {
        internal byte[] _items;
        internal int _size;
        internal byte* ptr;
        public int Count => _size;

        public UnsafeStream(int initCapacity = 4)
        {
            //Does not Free Handle
            _items = Array.Empty<byte>();
            _size = 0;
            Capacity = initCapacity;
        }

        public UnsafeStream(byte[] items)
        {
            _items = items;
            _size = items.Length;
        }

        public int Capacity
        {
            get => _items.Length;
            set => ptr = Resize(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte* Resize(int value)
        {
            if (value < _size)
                throw new JuliaException("Index to Small!");
            if (value != _items.Length)
            {
                if (value > 0)
                {
                    byte[] newItems = GC.AllocateUninitializedArray<byte>(value, true);
                    if (_size > 0)
                        Array.Copy(_items, newItems, _size);
                    _items = newItems;
                    fixed (byte* bptr = newItems)
                        return bptr;
                }

                _items = Array.Empty<byte>();
                _size = 0;
                return null;
            }

            return ptr;
        }

        public T[] ReadArray<T>(int idx) where T : unmanaged
        {
            T[] v = new T[Read<int>(idx)];
            idx += sizeof(int);
            fixed (T* dptr = v)
                Unsafe.CopyBlock(dptr, (T*)(ptr + idx), (uint)v.Length);
            return v;
        }

        public T* WriteArray<T>(T[] v, out int aptr) where T : unmanaged
        {
            var p = WriteArray<T>(v.Length, out aptr);
            fixed (T* dptr = v)
                Unsafe.CopyBlock(p, dptr, (uint)(sizeof(T) * v.Length));
            return p;
        }

        public T* WriteArray<T>(int size, out int aptr) where T : unmanaged
        {
            var p = (int*) WritePtr<T>(size, out aptr, sizeof(int));
            *p++ = size;
            return (T*) p;
        }

        public int WriteList<T>(UnsafeList<T> v) where T : unmanaged
        {
            int aptr;
            var p = WriteArray<T>(v.Count, out aptr);
            Unsafe.CopyBlock(p, v.GetDataPointer(sizeof(int)), (uint)(v.Count * sizeof(T)));
            return aptr;
        }

        public int WriteList<T>(List<T> v) where T : unmanaged
        {
            int aptr;
            var p = WriteArray<T>(v.Count, out aptr);
            for (int i = 0, n = v.Count; i < n; i++)
                p[i] = v[i];
            return aptr;
        }

        public T* WritePtr<T>(int nel, out int pptr, int pad = 0) where T : unmanaged {
            pptr = EnsureSizeWrite(_size + sizeof(T) * nel + pad);
            return (T*)(pptr + ptr);
        }

        public int Write<T>(ref T v) where T : unmanaged {
            *WritePtr<T>(1, out int aptr) = v;
            return aptr;
        }

        public int Write<T>(T v) where T : unmanaged {
            *WritePtr<T>(1, out int aptr) = v;
            return aptr;
        }

        protected int EnsureSizeWrite(int size)
        {
            byte[] array = _items;
            if ((uint)size >= (uint)array.Length) {
                int min = size + 1;
                if (_items.Length < min)
                {
                    int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;
                    if (newCapacity < min)
                        newCapacity = min;
                    Capacity = newCapacity;
                }
            }

            var loc = _size;
            _size = size;
            return loc;
        }

        public byte[] ToByteArray() {
            var bytes = new byte[Count];
            fixed(byte* dest = bytes)
                Unsafe.CopyBlock(dest, ptr, (uint) Count);
            return bytes;
        }

        public byte* GetDataPointer(int idx) => ptr + idx;

        public ref T Read<T>() where T : unmanaged => ref Read<T>(_size);
        public ref T Read<T>(int idx) where T : unmanaged => ref Unsafe.AsRef<T>(ptr + idx);
        public byte* GetDataPointer() => ptr;
        public void SetCount(int p) => _size = p;
    }

    public static class ListExt
    {
        public static bool Visit<T>(this IList<T> l, Func<T, bool> v)
        {
            foreach (var t in l)
                if (!v(t))
                    return false;
            return true;
        }
        
        public static bool Visit<T>(this T[] l, Func<T, bool> v)
        {
            foreach (var t in l)
                if (!v(t))
                    return false;
            return true;
        }

        public static void Println<T>(this IList<T> l)
        {
            foreach (var v in l)
                Console.WriteLine(v);
        }

        public static void Println<T>(this T[] l)
        {
            foreach (var v in l)
                Console.WriteLine(v);
        }
    }

    public class UnsafeList<T> : UnsafeStream, IList<T> where T : unmanaged
    {
        public UnsafeList() : base(sizeof(int)) => Write<int>(0);

        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public void Add(T item) =>  Write(item);
        public void Clear() => Capacity = 0;
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        public bool IsReadOnly => false;
        public int IndexOf(T item) => throw new NotImplementedException();
        public void Insert(int index, T item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        public void PopLast() => Capacity--;

        public T this[int index]
        {
            get => Read<T>(index + sizeof(int));
            set => Read<T>(index + sizeof(int)) = value;
        }

        public ref T GetRef(int index) => ref Read<T>(index + sizeof(int));
    }
}