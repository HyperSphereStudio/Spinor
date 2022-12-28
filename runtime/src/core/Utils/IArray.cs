using System.Collections;
using System.Collections.Generic;

namespace runtime.core.Utils;

public interface IArray<T> : IList<T>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    void ICollection<T>.Add(T item) => throw new System.NotImplementedException();
    void ICollection<T>.Clear() => throw new System.NotImplementedException();
    bool ICollection<T>.Contains(T item) => IndexOf(item) != -1;
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
        foreach (var v in this)
           array[arrayIndex++] = v;
    }

    bool ICollection<T>.Remove(T item) => throw new System.NotImplementedException();
    bool ICollection<T>.IsReadOnly => false;
    
    int IList<T>.IndexOf(T item) {
        for(int i = 0; i < Count; i++)
            if (item.Equals(this[i]))
                return i;
        return -1;
    }

    void IList<T>.Insert(int index, T item) => throw new System.NotImplementedException();
    void IList<T>.RemoveAt(int index) => throw new System.NotImplementedException();
}