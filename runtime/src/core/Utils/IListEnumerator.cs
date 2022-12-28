using System.Collections;
using System.Collections.Generic;

namespace runtime.core.Utils;

public class IListEnumerator<T> : IEnumerator<T>
{
    private readonly IList<T> _list;
    private int idx = 0;

    public IListEnumerator(IList<T> list) => _list = list;

    public bool MoveNext() => idx++ < _list.Count;
    public void Reset() => idx = 0;
    public T Current => _list[idx];
    object IEnumerator.Current => Current;
    public void Dispose() {}
}