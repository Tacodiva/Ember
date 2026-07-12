
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;

public unsafe readonly struct UnmanagedConcurrentHashSet<T> : ICollection<T>, IDisposable where T : unmanaged {

    public static UnmanagedConcurrentHashSet<T> Allocate(int capacity = 16, int buckets = 17) {
        return new UnmanagedConcurrentHashSet<T>(UnmanagedConcurrentHashTable<ValuePtr<T>, T, ValuePtr<T>, T, UnmanagedHashSet<T>.HashTableMethods>.Allocate(capacity, buckets));
    }

    private readonly UnmanagedConcurrentHashTable<ValuePtr<T>, T, ValuePtr<T>, T, UnmanagedHashSet<T>.HashTableMethods> _table;

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsNull => _table.IsNull;
    public readonly bool IsEmpty => Count == 0;

    private UnmanagedConcurrentHashSet(UnmanagedConcurrentHashTable<ValuePtr<T>, T, ValuePtr<T>, T, UnmanagedHashSet<T>.HashTableMethods> table) {
        _table = table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _table.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(in T item) =>  _table.InlineSet(ValuePtr<T>.Create(in item), false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T* item) =>  _table.InlineSet(item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item) =>  _table.InlineSet(&item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<T>.Add(T item) => _table.InlineSet(&item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in T item) =>  _table.InlineRemove(ValuePtr<T>.Create(in item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T* item) =>  _table.InlineRemove(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item) =>  _table.InlineRemove(&item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in T item) => _table.InlineContainsKey(ValuePtr<T>.Create(in item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => _table.InlineContainsKey(&item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T* item) => _table.InlineContainsKey(item);

    public void Clear() => _table.Clear();

    public Enumerator GetEnumerator() => new(_table.GetEnumerator());
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        private UnmanagedInlineConcurrentHashTable<ValuePtr<T>, T, ValuePtr<T>, T, UnmanagedHashSet<T>.HashTableMethods>.UnmanagedEnumerator _enumerator;

        public T Current => _enumerator.Current.Get();
        object IEnumerator.Current => Current;

        internal Enumerator(UnmanagedInlineConcurrentHashTable<ValuePtr<T>, T, ValuePtr<T>, T, UnmanagedHashSet<T>.HashTableMethods>.UnmanagedEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }

    public void CopyTo(MemorySpan<T> memory) {
        int i = 0;
        foreach (Ptr<T> value in _table.GetEnumerable(true)) {
            if (i >= _table.Count) throw new InvalidOperationException($"{nameof(memory)} not large enough."); 
            memory[i++] = value.Get();
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        int i = arrayIndex;
        foreach (Ptr<T> value in _table.GetEnumerable(true))
            array[i++] = value.Get();
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _table.DebugValidate();
    }
}