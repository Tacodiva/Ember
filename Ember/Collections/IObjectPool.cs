
using System.Collections;
using System.Collections.Generic;

namespace Ember;

public interface IObjectPool<T> : IEnumerable<T> {
    public int Count { get; }
    public T Borrow();
    public bool Return(T obj);
    public void Clear();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}