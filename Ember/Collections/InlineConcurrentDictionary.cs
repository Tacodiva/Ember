
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;


public struct InlineConcurrentDictionary<TKey, TValue> {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InlineConcurrentDictionary<TKey, TValue> Allocate(int capacity = 16) {
        Unsafe.SkipInit(out InlineConcurrentDictionary<TKey, TValue> dict);
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

    internal readonly struct ConcurrentHashTableMethods : IConcurrentHashTableMethods<RefReadonly<TKey>, TableValue, TableValueRef, TValue> {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DereferenceValue(TableValueRef valueRef, out TableValue destination) {
            destination.Key = valueRef.Key;
            destination.Value = UnsafeInterlocked.Read(in valueRef.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyHashCode(RefReadonly<TKey> key) {
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
        public static bool IsKeyValueEqual(RefReadonly<TKey> key, in TableValue value) {
            return EqualityComparer<TKey>.Default.Equals(key.Value, value.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueRefValueEqual(TableValueRef valueRef, in TableValue value) {
            return EqualityComparer<TKey>.Default.Equals(valueRef.Key, value.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyValueOut(in TableValue value, out TValue valueOut) {
            valueOut = UnsafeInterlocked.Read(in value.Value);
        }

        public static bool HasAtomicValueOverwrite => Unsafe.SizeOf<TValue>() == 4 || Unsafe.SizeOf<TValue>() == 8;

        public static void AtomicValueOverwrite(TableValueRef valueRef, ref TableValue value) {
            UnsafeInterlocked.Write(ref value.Value, valueRef.Value);
        }
    }

    private InlineConcurrentHashTable<RefReadonly<TKey>, TableValue, TableValueRef, TValue, ConcurrentHashTableMethods> _table;

    public readonly int Count => _table.Count;

    public readonly bool IsReadOnly => false;
    public readonly bool IsEmpty => Count == 0;

    public void Init(int capacity = 16) {
        _table.Init(capacity);
    }

    public void Add(in TKey key, in TValue value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (!_table.InlineTryAdd(new(in key, in value)))
            throw new InvalidOperationException($"Key already present in dictionary.");
    }

    public void Set(in TKey key, in TValue value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        _table.InlineSet(new(in key, in value));
    }

    public bool Remove(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        return _table.InlineRemove(new(in key));
    }

    public bool Remove(in TKey key, [NotNullWhen(true)] out TValue? value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        Unsafe.SkipInit(out value);
        if (_table.InlineRemove(new(in key), ref value))
            return true;
        value = default;
        return false;
    }

    public void Clear() => _table.InlineClear();

        // [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGetValueAAA(in TKey key, [NotNullWhen(true)] out TValue? value) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        Unsafe.SkipInit(out value);

        if (_table.InlineTryGet(new(in key), ref value))
            return true;

        value = default;
        return false;
    }

    public TValue GetValue(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        Unsafe.SkipInit(out TValue? value);
        if (!_table.InlineTryGet(new(in key), ref value))
            throw new KeyNotFoundException("Key not present in dictionary.");
        return value;
    }

    public TValue GetValueOrDefault(in TKey key, in TValue defaultValue) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (TryGetValueAAA(key, out TValue? value))
            return value;
        return defaultValue;
    }

    public TValue this[in TKey key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetValue(in key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Set(in key, value);
    }

    public bool ContainsKey(in TKey key) {
        if (key is null) throw new ArgumentNullException(nameof(key));
        return _table.InlineContainsKey(new(in key));
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (item.Key is null) throw new ArgumentNullException($"{nameof(item)}.{nameof(item.Key)}");
        if (!TryGetValueAAA(item.Key, out TValue? value))
            return false;
        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }
}