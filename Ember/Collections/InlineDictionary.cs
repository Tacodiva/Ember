
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Collections;

public struct InlineDictionary<TKey, TValue> {


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InlineDictionary<TKey, TValue> Allocate(int capacity = 16) {
        InlineDictionary<TKey, TValue> dict = default;
        dict.Init(capacity);
        return dict;
    }

    internal ref struct TableValueRef(ref readonly TKey key, ref readonly TValue value) {
        public ref readonly TKey Key = ref key;
        public ref readonly TValue Value = ref value;
    }

    internal struct TableValue(TKey key, TValue value) {
        public TKey Key = key;
        public TValue Value = value;

        public static implicit operator KeyValuePair<TKey, TValue>(TableValue v) => new(v.Key, v.Value);
    }

    internal readonly struct HashTableMethods : IHashTableMethods<Ref<TKey>, TableValue, TableValueRef> {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DereferenceValue(TableValueRef valueRef, out TableValue destination) {
            destination.Key = valueRef.Key;
            destination.Value = valueRef.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyHashCode(Ref<TKey> key) {
            return key.Value!.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValueHashCode(in TableValue value) {
            return value.Key!.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValueRefHashCode(TableValueRef valueRef) {
            return valueRef.Key!.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyValueEqual(Ref<TKey> key, in TableValue value) {
            return EqualityComparer<TKey>.Default.Equals(key.Value, value.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueRefValueEqual(TableValueRef valueRef, in TableValue value) {
            return EqualityComparer<TKey>.Default.Equals(valueRef.Key, value.Key);
        }
    }

    private InlineHashTable<Ref<TKey>, TableValue, TableValueRef, HashTableMethods> _table;

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsEmpty => Count == 0;

    public void Init(int capacity = 16) {
        _table.Init(capacity);
    }

    public void Add(in TKey key, in TValue value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (!_table.InlineAdd(new(ref Unsafe.AsRef(in key), ref Unsafe.AsRef(in value))))
            throw new InvalidOperationException($"Key already present in dictionary.");
    }

    public void Set(in TKey key, in TValue value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        _table.InlineSet(new(ref Unsafe.AsRef(in key), ref Unsafe.AsRef(in value)));
    }

    public bool Remove(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        return _table.InlineRemove(new(ref Unsafe.AsRef(in key)));
    }

    public bool Remove(in TKey key, [NotNullWhen(true)] out TValue? value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        Unsafe.SkipInit(out TableValue tableValue);
        bool success = _table.InlineRemove(new(ref Unsafe.AsRef(in key)), ref tableValue);
        if (success) value = tableValue.Value;
        else value = default;
        return success;
    }

    public void Clear() => _table.InlineClear();
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue? value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        ref TableValue tableValue = ref _table.InlineGet(new(ref Unsafe.AsRef(in key)));
        if (Unsafe.IsNullRef(ref tableValue)) {
            value = default;
            return false;
        } else {
            value = tableValue.Value;
            return true;
        }
    }

    [UnscopedRef]
    public ref TValue TryGetValueReference(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        ref TableValue tableValue = ref _table.InlineGet(new(ref Unsafe.AsRef(in key)));
        if (Unsafe.IsNullRef(ref tableValue)) return ref Unsafe.NullRef<TValue>();
        return ref tableValue.Value;
    }

    [UnscopedRef]
    public ref TValue GetValueReference(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        ref TableValue tableValue = ref _table.InlineGet(new(ref Unsafe.AsRef(in key)));
        if (Unsafe.IsNullRef(ref tableValue))
            throw new KeyNotFoundException("Key not present in dictionary.");
        return ref tableValue.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetValue(in TKey key) => GetValueReference(key);

    public TValue GetValueOrDefault(in TKey key, in TValue defaultValue) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        ref TableValue tableValue = ref _table.InlineGet(new(ref Unsafe.AsRef(in key)));
        if (Unsafe.IsNullRef(ref tableValue)) return defaultValue;
        return tableValue.Value;
    }

    public TValue this[in TKey key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetValue(in key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Set(in key, in value);
    }

    public bool ContainsKey(in TKey key) => _table.InlineContainsKey(new(ref Unsafe.AsRef(in key)));

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (!TryGetValue(item.Key, out TValue? value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _table.DebugValidate();
    }
}