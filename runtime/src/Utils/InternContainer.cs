/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;

namespace runtime.Utils;

internal struct InternElement {
    internal int Hash;
    internal int Idx;
}

internal class InternElementEquality<T> : IEqualityComparer<InternElement> where T: class{
    private readonly InternContainer<T> _c;
    internal InternElementEquality(InternContainer<T> c) => _c = c;
    public bool Equals(InternElement x, InternElement y) => 
        x.Hash == y.Hash && (x.Idx == -1 ? Equals(_c.CheckItem, _c.Data[y.Idx]) : y.Idx == -1 && Equals(_c.CheckItem, _c.Data[x.Idx]));

    public int GetHashCode(InternElement obj) => obj.Hash;
}

public class InternContainer<T> where T: class{
    private readonly HashSet<InternElement> _s;
    internal readonly List<T> Data = new();
    internal T CheckItem;
        
    public InternContainer() => _s = new(new InternElementEquality<T>(this));

    public T Get(int idx) => Data[idx];
    public void Set(int idx, T v) {
        InternElement e = new() {
            Hash = Data[idx]?.GetHashCode() ?? 0,
            Idx = idx
        };
        _s.Remove(e);
        Data[idx] = v;
        e.Hash = v.GetHashCode();
        _s.Add(e);
    }
    public int Load(T v) {
        InternElement e = new() {
            Hash = v?.GetHashCode() ?? 0,
            Idx = -1
        };
        CheckItem = v;
        if (_s.TryGetValue(e, out var rE))
            return rE.Idx;
        e.Idx = Data.Count;
        Data.Add(v);
        _s.Add(e);
        return e.Idx;
    }
    public int GetIndex(T v) {
        InternElement e = new() {
            Hash = v?.GetHashCode() ?? 0,
            Idx = -1
        };
        CheckItem = v;
        if (_s.TryGetValue(e, out var rE))
            return rE.Idx;
        return -1;
    }
}