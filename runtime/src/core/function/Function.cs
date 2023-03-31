/*
   * Author : Johnathan Bizzano
   * Created : Thursday, March 16, 2023
   * Last Modified : Thursday, March 16, 2023
*/

using System;
using System.Collections.Generic;
using System.Linq;
using runtime.stdlib;

namespace runtime.core.function;

public record Function(Symbol Name, Module Module) : IComparer<int>{
    public readonly List<Method> Methods = new();
    private readonly Dictionary<int, IMethodCollection> _methodsByParameterCount = new ();

    public void AddMethod(Method m) {
        var fM = FindMethodIndex(m.Signature);
        if (fM != -1) {
            Methods[fM] = m;
            return;
        }

        var idx = Methods.Count;
        var pCount = m.Signature.Args.Length;
        Methods.Add(m);
        if (!_methodsByParameterCount.TryGetValue(pCount, out var c)) {
            c = new BruteForceMethodCollection(this);
            _methodsByParameterCount.Add(pCount, c);
        }
        
        c.Add(idx);
    }

    public Any Invoke(params Any[] values) {
        var sig = new MethodSignature(values.Select(x => x.Type).ToArray());
        var m = FindMethod(sig);
        if (m == null)
            throw new SpinorException($"Unable To Invoke Method {Name} for Arguments {sig}");
        return m.Invoke(values);
    }

    private int FindMethodIndex(MethodSignature sig) {
        if (!_methodsByParameterCount.TryGetValue(sig.Args.Length, out var mc))
            return -1;
        return mc.Match(sig);
    }

    public Method FindMethod(MethodSignature sig) => Methods[FindMethodIndex(sig)];

    public int Compare(int x, int y) => Methods[x].Signature.Specificity - Methods[y].Signature.Specificity;

    private interface IMethodCollection {
        public void Add(int idx);
        public int Match(MethodSignature sig);
    }

    //Sort Methods Through Brute Force Search
    private class BruteForceMethodCollection : SortedSet<int>, IMethodCollection {
        public BruteForceMethodCollection(Function f) : base(f){}

        void IMethodCollection.Add(int idx) => base.Add(idx);
        int IMethodCollection.Match(MethodSignature sig) {
            var f = (Function) Comparer;
            var itr = GetEnumerator();
            
            //Skip All Methods With Greater Specificity
            while(itr.MoveNext() && f.Methods[itr.Current].Signature.Specificity > sig.Specificity){}

            while (itr.MoveNext()) {
                if (!f.Methods[itr.Current].Signature.Match(sig))
                    continue;

                var method = itr.Current;
                itr.Dispose();
                return method;
            }

            itr.Dispose();
            return -1;
        }
    }
}