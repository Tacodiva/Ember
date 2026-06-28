
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Utils;

namespace Ember.Collections;

public unsafe struct UnmanagedInlineDictionary<TKey, TValue> : IDisposable where TKey : unmanaged where TValue : unmanaged {

    internal struct TableValueRef(ValuePtr<TKey> key, ValuePtr<TValue> value) {
        public ValuePtr<TKey> Key = key;
        public ValuePtr<TValue> Value = value;
    }

    internal struct TableValue(TKey key, TValue value) {
        public TKey Key = key;
        public TValue Value = value;

        public static implicit operator KeyValuePair<TKey, TValue>(TableValue v) => new(v.Key, v.Value);
    }

    private UnmanagedInlineHashTable<ValuePtr<TKey>, TableValue, TableValueRef, HashTableMethods> _table;

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
    public readonly bool IsEmpty => Count == 0;

    public void Init() {
        _table.Init();
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

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (!TryGetValue(item.Key, out TValue value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
        return Remove(item.Key);
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _table.DebugValidate();
    }
}