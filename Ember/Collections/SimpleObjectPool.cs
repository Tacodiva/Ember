
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Ember;

public abstract class SimpleObjectPool<T> : IObjectPool<T> {

    public readonly int MaxCount;
    public abstract int Count { get; }

    private readonly Func<T> _factory;

    public SimpleObjectPool(Func<T> factory, int maxCount) {
        _factory = factory;
        MaxCount = maxCount;
    }

    public T Borrow() {
        if (TryTake(out T? result))
            return result;
        return _factory();
    }

    public bool Return(T obj) {
        if (MaxCount > 0 && Count >= MaxCount)
            return false;
#if EMBER_SAFETY_CHECKS
        if (this.Contains(obj))
            throw new InvalidOperationException("Object already returned.");
#endif
        return TryAdd(obj);
    }

    public abstract void Clear();
    protected abstract bool TryTake([MaybeNullWhen(false)] out T result);
    protected abstract bool TryAdd(T obj);
    public abstract IEnumerator<T> GetEnumerator();
}