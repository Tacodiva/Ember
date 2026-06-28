
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Logging;
using Ember.Memory;

namespace Ember.Collections;

public unsafe class FreeableList<T> : IEnumerable<T>, IDisposable {

    private int _count;
    private readonly List<T> _list;
    private readonly UnmanagedIndexAllocator _indexAllocator;

    public int Count => _count;

    public FreeableList() {
        _count = 0;
        _list = [];
        _indexAllocator = UnmanagedIndexAllocator.Allocate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index) => _list[index];

    public int Add(in T item) {
        int index = _indexAllocator.AllocateIndex();
        ++_count;

        Debug.Assert(index <= _list.Count);

        if (index == _list.Count) {
            _list.Add(item);
        } else {
            _list[index] = item;
        }

        return index;
    }

    public void Free(int index) {
        _indexAllocator.FreeIndex(index);
        _list[index] = default!;
        --_count;
    }

    public bool IsIndexAllocated(int index) {
        return _indexAllocator.IsIndexAllocated(index);
    }

    public void Clear() {
        _indexAllocator.Clear();
        _list.Clear();
        _count = 0;
    }

    public T this[int index] {
        get => Get(index);
        set => _list[index] = value;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _indexAllocator.Dispose();
    }

    ~FreeableList() {
        Dispose();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(this);

    public struct Enumerator : IEnumerator<T> {
        private readonly UnmanagedIndexAllocator _indexAllocator;
        private readonly List<T> _list;
        public int Index { get; private set; }

        public readonly T Current => _list[Index];
        readonly object? IEnumerator.Current => Current;

        public Enumerator(FreeableList<T> list) {
            _indexAllocator = list._indexAllocator;
            _list = list._list;
            Reset();
        }

        public bool MoveNext() {
            bool hasNext;
            do {
                hasNext = ++Index != _list.Count;
            } while (hasNext && !_indexAllocator.IsIndexAllocated(Index));

            return hasNext;
        }

        public void Reset() {
            Index = -1;
        }

        public void Dispose() { }
    }
}