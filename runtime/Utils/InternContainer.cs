using System;
using System.Collections.Generic;

namespace runtime.Utils
{
    internal struct InternElement {
        internal int hash;
        internal int idx;
    }

    internal class MInternElementEquality<T> : IEqualityComparer<InternElement> {
        private MInternContainer<T> _c;
        
        internal MInternElementEquality(MInternContainer<T> c) => _c = c;
        public bool Equals(InternElement x, InternElement y) {
            return x.hash == y.hash && (x.idx == -1 ? _c.checkItem.Equals(_c.Data[y.idx]) : y.idx == -1 && _c.checkItem.Equals(_c.Data[x.idx]));
        }

        public int GetHashCode(InternElement obj) => obj.hash;
    }

    public class MInternContainer<T>
    {
        internal HashSet<InternElement> S;
        internal List<T> Data = new();
        internal T checkItem;
        
        public MInternContainer() => S = new(new MInternElementEquality<T>(this));

        public T Get(int idx) => Data[idx];
        
        public void Set(int idx, T v) {
            InternElement e = new();
            e.hash = Data[idx].GetHashCode();
            e.idx = idx;
            S.Remove(e);
            Data[idx] = v;
            e.hash = v.GetHashCode();
            S.Add(e);
        }

        public int Load(T v)
        {
            InternElement e = new();
            e.hash = v.GetHashCode();
            e.idx = -1;
            checkItem = v;
            if (S.TryGetValue(e, out var rE))
                return rE.idx;
            e.idx = Data.Count;
            Data.Add(v);
            S.Add(e);
            return e.idx;
        }

        public int GetIndex(T v) {
            InternElement e = new();
            e.hash = v.GetHashCode();
            e.idx = -1;
            checkItem = v;
            if (S.TryGetValue(e, out var rE))
                return rE.idx;
            return -1;
        }
    }
    
    internal class UInternElementEquality<T> : IEqualityComparer<InternElement> where T: unmanaged{
        private UInternContainer<T> _c;
        
        internal UInternElementEquality(UInternContainer<T> c) => _c = c;
        public bool Equals(InternElement x, InternElement y) {
            return x.hash == y.hash && (x.idx == -1 ? _c.CheckItem.Equals(_c._data[y.idx]) : y.idx == -1 && _c.CheckItem.Equals(_c._data[x.idx]));
        }

        public int GetHashCode(InternElement obj) => obj.hash;
    }
    
    public class UInternContainer<T> where T: unmanaged
    {
        private readonly HashSet<InternElement> _s;
        internal readonly UnsafeList<T> _data = new();
        internal T CheckItem = default;
        
        public UInternContainer() => _s = new(new UInternElementEquality<T>(this));

        public ref T Get(int idx) => ref _data.GetRef(idx);

        public void Set(int idx, T v) {
            InternElement e = new();
            e.hash = _data[idx].GetHashCode();
            e.idx = idx;
            _s.Remove(e);
            _data[idx] = v;
            e.hash = v.GetHashCode();
            _s.Add(e);
        }
        
        public int Load(T v) {
            InternElement e = new();
            e.hash = v.GetHashCode();
            e.idx = -1;
            CheckItem = v;
            if (_s.TryGetValue(e, out var rE))
                return rE.idx;
            e.idx = _data.Count;
            _data.Add(v);
            _s.Add(e);
            return e.idx;
        }
        
    }
}