using System.Collections.Generic;

namespace runtime.Utils
{
    public struct InternedHashElement<T> {
        public readonly T Data;
        public int Idx;

        public InternedHashElement(T data, int idx)
        {
            Data = data;
            Idx = idx;
        }
    }
    internal struct InternedEqualityComparer<T> : IEqualityComparer<InternedHashElement<T>>
    {
        public int GetHashCode(InternedHashElement<T> obj) => obj.Data.GetHashCode();
        public bool Equals(InternedHashElement<T> obj1, InternedHashElement<T> obj2) => obj1.Equals(obj2);
    }
    internal class InternedCollection<T>
    {
        private readonly HashSet<InternedHashElement<T>> DataCheckingSet;
        internal readonly List<T> DataList;

        public InternedCollection() {
            DataCheckingSet = new(new InternedEqualityComparer<T>());
            DataList = new();
        }

        public bool Contains(T data) => DataCheckingSet.Contains(new(data, 0));
        
        public T Get(int i) => DataList[i];
            
        public int Load(T data)
        {
            InternedHashElement<T> e = new(data, 0);
            if (DataCheckingSet.TryGetValue(e, out InternedHashElement<T> v))
                return v.Idx;
            e.Idx = DataList.Count;
            DataList.Add(data);
            DataCheckingSet.Add(e);
            return e.Idx;
        }
    }
}