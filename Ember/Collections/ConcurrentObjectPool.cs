
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ember.Collections;

public class ConcurrentObjectPool<T> : SimpleObjectPool<T> {

    private readonly ConcurrentStack<T> _pool;
    public override int Count => _pool.Count;

    public ConcurrentObjectPool(Func<T> factory, int initCount = 0, int maxCount = -1) : base(factory, maxCount) {
        _pool = new();

        for (int i = 0; i < initCount; i++)
            _pool.Push(factory());
    }

    public override IEnumerator<T> GetEnumerator() => _pool.GetEnumerator();

    protected override bool TryAdd(T obj) {
        _pool.Push(obj);
        return true;
    }

    protected override bool TryTake([MaybeNullWhen(false)] out T result) {
        return _pool.TryPop(out result);
    }

    public override void Clear() {
        _pool.Clear();
    }
}