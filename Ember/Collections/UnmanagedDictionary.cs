
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Utils;

namespace Ember.Collections;

public unsafe readonly struct UnmanagedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable where TKey : unmanaged where TValue : unmanaged {

    public static UnmanagedDictionary<TKey, TValue> Allocate(int capacity = 16, int buckets = 17) {
        return new UnmanagedDictionary<TKey, TValue>(UnmanagedHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods>.Allocate(capacity, buckets));
    }

    internal struct TableValueRef(ValuePtr<TKey> key, ValuePtr<TValue> value) {
        public ValuePtr<TKey> Key = key;
        public ValuePtr<TValue> Value = value;
    }

    internal struct TableValue(TKey key, TValue value) {
        public TKey Key = key;
        public TValue Value = value;

        public static implicit operator KeyValuePair<TKey, TValue>(TableValue v) => new(v.Key, v.Value);
    }

    private readonly UnmanagedHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods> _table;

    internal readonly struct HashTableMethods : IHashTableMethods<ValuePtr<TKey>, TableValue, TableValueRef> {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetKeyHashCode(ValuePtr<TKey> key) => (uint)key.Get().GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetValueHashCode(TableValue* value) => (uint)value->Key.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyValueEqual(ValuePtr<TKey> key, TableValue* value) {
            return EqualityComparer<TKey>.Default.Equals(key, value->Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueEqual(TableValue* a, TableValue* b) {
            return EqualityComparer<TKey>.Default.Equals(a->Key, b->Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetValueRefHashCode(TableValueRef valueRef) {
            return (uint)valueRef.Key.Get().GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueRefValueEqual(TableValueRef valueRef, TableValue* value) {
            return EqualityComparer<TKey>.Default.Equals(valueRef.Key, value->Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DereferenceValue(TableValueRef valueRef, TableValue* destination) {
            destination->Key = valueRef.Key;
            destination->Value = valueRef.Value;
        }
    }

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsNull => _table.IsNull;
    public readonly bool IsEmpty => Count == 0;

    private UnmanagedDictionary(UnmanagedHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods> table) {
        _table = table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _table.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) => Add(key, in value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, in TValue value) => Add(key, (TValue*)Unsafe.AsPointer(ref Unsafe.AsRef(in value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue* value) {
        if (!_table.InlineSet(new(&key, value), false))
            throw new InvalidOperationException($"Key already present in dictionary.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, TValue value) => Set(key, in value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, in TValue value) {
        _table.InlineSet(new(&key, ValuePtr<TValue>.Create(in value)), true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, TValue* value) {
        _table.InlineSet(new(&key, value), true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) => _table.InlineRemove(&key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, out TValue value) {
        TableValue tableValue;
        bool success = _table.InlineRemove(&key, &tableValue);
        if (success) value = tableValue.Value;
        else value = default;
        return success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, TValue* outValue) {
        TableValue tableValue;
        bool success = _table.InlineRemove(&key, &tableValue);
        if (success) *outValue = tableValue.Value;
        return success;
    }

    public void Clear() => _table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        TableValue* tableValue = _table.InlineGet(&key);
        if (tableValue == null) {
            value = default;
            return false;
        } else {
            value = tableValue->Value;
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue* TryGetValuePointer(TKey key) {
        TableValue* tableValue = _table.InlineGet(&key);
        return tableValue == null ? null : &tableValue->Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue* GetValuePointer(TKey key) {
        TValue* value = TryGetValuePointer(key);
        if (value == null)
            throw new KeyNotFoundException("Key not present in dictionary.");
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetValue(TKey key) {
        return *GetValuePointer(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetValueOrDefault(TKey key, TValue defaultValue = default) {
        TValue* value = TryGetValuePointer(key);
        if (value == null) return defaultValue;
        return *value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue TryGetValueRef(TKey key) {
        return ref Unsafe.AsRef<TValue>(TryGetValuePointer(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValueRef(TKey key) {
        return ref Unsafe.AsRef<TValue>(GetValuePointer(key));
    }

    public TValue this[TKey key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetValue(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _table.InlineSet(new(&key, &value), true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => _table.InlineContainsKey(&key);

    public bool ContainsValue(in TValue value) {
        foreach (KeyValuePair<TKey, TValue> kvp in this)
            if (EqualityComparer<TValue>.Default.Equals(value, kvp.Value))
                return true;
        return false;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (!TryGetValue(item.Key, out TValue value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        int i = arrayIndex;
        foreach (KeyValuePair<TKey, TValue> kvp in this)
            array[i++] = kvp;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
        return Remove(item.Key);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(_table.GetEnumerator());

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
        private UnmanagedInlineHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods>.UnmanagedEnumerator _enumerator;

        public KeyValuePair<TKey, TValue> Current => _enumerator.Current.Get();
        object IEnumerator.Current => Current;

        internal Enumerator(UnmanagedInlineHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods>.UnmanagedEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
        public readonly void Dispose() => _enumerator.Dispose();
    }

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeyCollection(this);
    // This is why people hate OO programing lmao
    public readonly struct KeyCollection(UnmanagedDictionary<TKey, TValue> dictionary) : ICollection<TKey> {
        public readonly UnmanagedDictionary<TKey, TValue> Dictionary = dictionary;
        public int Count => Dictionary.Count;

        public bool Contains(TKey item) => Dictionary.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex) {
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                array[arrayIndex++] = kvp.Key;
        }

        public IEnumerator<TKey> GetEnumerator() => new KeyEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct KeyEnumerator(UnmanagedDictionary<TKey, TValue>.KeyCollection keys) : IEnumerator<TKey> {
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

    public readonly struct ValueCollection(UnmanagedDictionary<TKey, TValue> dictionary) : ICollection<TValue> {
        public readonly UnmanagedDictionary<TKey, TValue> Dictionary = dictionary;
        public int Count => Dictionary.Count;

        public bool Contains(TValue item) => Dictionary.ContainsValue(item);

        public void CopyTo(TValue[] array, int arrayIndex) {
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                array[arrayIndex++] = kvp.Value;
        }

        public IEnumerator<TValue> GetEnumerator() => new ValueEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct ValueEnumerator(UnmanagedDictionary<TKey, TValue>.ValueCollection keys) : IEnumerator<TValue> {
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