
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Utils;

namespace Ember.Collections;

public unsafe readonly struct UnmanagedConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable where TKey : unmanaged where TValue : unmanaged {

    public static UnmanagedConcurrentDictionary<TKey, TValue> Allocate(int capacity = 16, int buckets = 17) {
        return new(
            UnmanagedConcurrentHashTable<
                ValuePtr<TKey>,
                UnmanagedDictionary<TKey, TValue>.TableValue,
                UnmanagedDictionary<TKey, TValue>.TableValueRef,
                UnmanagedDictionary<TKey, TValue>.HashTableMethods
            >.Allocate(capacity, buckets)
        );
    }

    private readonly UnmanagedConcurrentHashTable<
        ValuePtr<TKey>,
        UnmanagedDictionary<TKey, TValue>.TableValue,
        UnmanagedDictionary<TKey, TValue>.TableValueRef,
        UnmanagedDictionary<TKey, TValue>.HashTableMethods
    > _table;

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsNull => _table.IsNull;
    public readonly bool IsEmpty => Count == 0;

    private UnmanagedConcurrentDictionary(UnmanagedConcurrentHashTable<
        ValuePtr<TKey>,
        UnmanagedDictionary<TKey, TValue>.TableValue,
        UnmanagedDictionary<TKey, TValue>.TableValueRef,
        UnmanagedDictionary<TKey, TValue>.HashTableMethods
    > table) {
        _table = table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _table.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(KeyValuePair<TKey, TValue> item) => TryAdd(item.Key, item.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TKey key, TValue value) => TryAdd(key, in value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TKey key, in TValue value) {
        return _table.InlineSet(new(&key, ValuePtr<TValue>.Create(in value)), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) => Add(key, in value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, in TValue value) {
        if (!TryAdd(key, value))
            throw new InvalidOperationException($"Key already present in dictionary.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, TValue value) => Set(key, in value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, in TValue value) {
        _table.InlineSet(new(&key, ValuePtr<TValue>.Create(in value)), true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) => _table.InlineRemove(&key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, out TValue value) {
        UnmanagedDictionary<TKey, TValue>.TableValue tableValue;
        bool success = _table.InlineRemove(&key, &tableValue);
        if (success) value = tableValue.Value;
        else value = default;
        return success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, TValue* outValue) {
        UnmanagedDictionary<TKey, TValue>.TableValue tableValue;
        bool success = _table.InlineRemove(&key, &tableValue);
        if (success) *outValue = tableValue.Value;
        return success;
    }

    public void Clear() => _table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        UnmanagedDictionary<TKey, TValue>.TableValue tableValue;

        if (!_table.InlineTryGet(&key, &tableValue)) {
            value = default;
            return false;
        }

        value = tableValue.Value;
        return true;
    }

    public TValue GetValue(TKey key) {
        UnmanagedDictionary<TKey, TValue>.TableValue tableValue;

        if (!_table.InlineTryGet(&key, &tableValue))
            throw new KeyNotFoundException("Key not present in dictionary.");

        return tableValue.Value;
    }

    public TValue this[TKey key] {
        get => GetValue(key);
        set => _table.InlineSet(new(&key, ValuePtr<TValue>.Create(in value)), true);
    }

    public bool ContainsKey(TKey key) => _table.InlineContainsKey(&key);

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (!TryGetValue(item.Key, out TValue value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    private bool ContainsValue(in TValue value) {
        foreach (KeyValuePair<TKey, TValue> kvp in this)
            if (EqualityComparer<TValue>.Default.Equals(value, kvp.Value))
                return true;
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        int i = arrayIndex;
        foreach (KeyValuePair<TKey, TValue> kvp in this)
            array[i++] = kvp;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(_table.GetEnumerator());

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {

        private UnmanagedInlineConcurrentHashTable<
            ValuePtr<TKey>,
            UnmanagedDictionary<TKey, TValue>.TableValue,
            UnmanagedDictionary<TKey, TValue>.TableValueRef,
            UnmanagedDictionary<TKey, TValue>.HashTableMethods
        >.Enumerator _enumerator;

        public KeyValuePair<TKey, TValue> Current => _enumerator.Current.Get();
        object IEnumerator.Current => Current;

        internal Enumerator(UnmanagedInlineConcurrentHashTable<
            ValuePtr<TKey>,
            UnmanagedDictionary<TKey, TValue>.TableValue,
            UnmanagedDictionary<TKey, TValue>.TableValueRef,
            UnmanagedDictionary<TKey, TValue>.HashTableMethods
        >.Enumerator enumerator) {
            _enumerator = enumerator;
        }

        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
        public readonly void Dispose() => _enumerator.Dispose();
    }

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeyCollection(this);
    // This is why people hate OO programing lmao
    public readonly struct KeyCollection(UnmanagedConcurrentDictionary<TKey, TValue> dictionary) : ICollection<TKey> {
        public readonly UnmanagedConcurrentDictionary<TKey, TValue> Dictionary = dictionary;
        public int Count => Dictionary.Count;

        public bool Contains(TKey item) => Dictionary.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex) {
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                array[arrayIndex++] = kvp.Key;
        }

        public IEnumerator<TKey> GetEnumerator() => new KeyEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct KeyEnumerator(UnmanagedConcurrentDictionary<TKey, TValue>.KeyCollection keys) : IEnumerator<TKey> {
            public Enumerator Enumerator = keys.Dictionary.GetEnumerator();

            public TKey Current => Enumerator.Current.Key;
            object IEnumerator.Current => Current;
            public readonly void Dispose() => Enumerator.Dispose();
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }

        public bool IsReadOnly => true;
        public void Add(TKey item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Remove(TKey item) => throw new NotSupportedException();
    }

    public ValueCollection Values => new(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    public readonly struct ValueCollection(UnmanagedConcurrentDictionary<TKey, TValue> dictionary) : ICollection<TValue> {
        public readonly UnmanagedConcurrentDictionary<TKey, TValue> Dictionary = dictionary;
        public int Count => Dictionary.Count;

        public bool Contains(TValue item) => Dictionary.ContainsValue(item);

        public void CopyTo(TValue[] array, int arrayIndex) {
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                array[arrayIndex++] = kvp.Value;
        }

        public IEnumerator<TValue> GetEnumerator() => new ValueEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct ValueEnumerator(UnmanagedConcurrentDictionary<TKey, TValue>.ValueCollection keys) : IEnumerator<TValue> {
            public Enumerator Enumerator = keys.Dictionary.GetEnumerator();

            public TValue Current => Enumerator.Current.Value;
            object IEnumerator.Current => Current;
            public readonly void Dispose() => Enumerator.Dispose();
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }

        public bool IsReadOnly => true;
        public void Add(TValue item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Remove(TValue item) => throw new NotSupportedException();
    }


    [Conditional("DEBUG")]
    public void DebugValidate() {
        _table.DebugValidate();
    }
}