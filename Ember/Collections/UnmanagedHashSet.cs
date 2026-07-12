
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Utils;

namespace Ember.Collections;

public unsafe readonly struct UnmanagedHashSet<T> : ISet<T>, IDisposable where T : unmanaged {

    public static UnmanagedHashSet<T> Allocate(int capacity = 16, int buckets = 17) {
        return new UnmanagedHashSet<T>(UnmanagedHashTable<ValuePtr<T>, T, ValuePtr<T>, HashTableMethods>.Allocate(capacity, buckets));
    }

    private readonly UnmanagedHashTable<ValuePtr<T>, T, ValuePtr<T>, HashTableMethods> _table;

    internal readonly struct HashTableMethods : IConcurrentHashTableMethods<ValuePtr<T>, T, ValuePtr<T>, T> {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetKeyHashCode(ValuePtr<T> key) => (uint)key.Get().GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetValueHashCode(T* value) => (uint)value->GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyValueEqual(ValuePtr<T> key, T* value) {
            return EqualityComparer<T>.Default.Equals(key, *value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueEqual(T* a, T* b) {
            return EqualityComparer<T>.Default.Equals(*a, *b);
        }

        public static uint GetValueRefHashCode(ValuePtr<T> valueRef) {
            return (uint) valueRef.Get().GetHashCode();
        }

        public static bool IsValueRefValueEqual(ValuePtr<T> valueRef, T* value) {
            return EqualityComparer<T>.Default.Equals(valueRef, *value);
        }

        public static void DereferenceValue(ValuePtr<T> valueRef, T* destination) {
            *destination = valueRef;
        }

        public static void CopyValueOut(T* value, T* valueOut) {
            *valueOut = *value;
        }
    }

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsNull => _table.IsNull;
    public readonly bool IsEmpty => Count == 0;

    private UnmanagedHashSet(UnmanagedHashTable<ValuePtr<T>, T, ValuePtr<T>, HashTableMethods> table) {
        _table = table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _table.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(in T item) => _table.InlineSet(ValuePtr<T>.Create(item), false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T* item) => _table.InlineSet(item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item) => _table.InlineSet(&item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<T>.Add(T item) => _table.InlineSet(&item, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in T item) => _table.InlineRemove(ValuePtr<T>.Create(item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T* item) => _table.InlineRemove(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item) => _table.InlineRemove(&item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in T item) => _table.InlineContainsKey(ValuePtr<T>.Create(item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T* item) => _table.InlineContainsKey(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => _table.InlineContainsKey(&item);

    public void Clear() => _table.Clear();

    public Enumerator GetEnumerator() => new(_table.GetEnumerator());
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        private UnmanagedInlineHashTable<ValuePtr<T>, T, ValuePtr<T>, HashTableMethods>.UnmanagedEnumerator _enumerator;

        public T Current => _enumerator.Current.Get();
        object IEnumerator.Current => Current;

        internal Enumerator(UnmanagedInlineHashTable<ValuePtr<T>, T, ValuePtr<T>, HashTableMethods>.UnmanagedEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }

    public void CopyTo(T[] array, int arrayIndex) {
        int i = arrayIndex;
        foreach (T value in this)
            array[i++] = value;
    }

    public void ExceptWith(IEnumerable<T> other) {
        foreach (T value in other) _table.InlineRemove(&value);
    }

    public void IntersectWith(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public bool IsProperSupersetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public bool IsSubsetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public bool IsSupersetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public bool Overlaps(IEnumerable<T> other) {
        foreach (T value in other)
            if (_table.InlineContainsKey(&value)) return true;
        return false;
    }

    public bool SetEquals(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public void SymmetricExceptWith(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    public void UnionWith(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _table.DebugValidate();
    }
}