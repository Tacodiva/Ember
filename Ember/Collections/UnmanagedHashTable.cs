
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Collections;

internal unsafe readonly struct UnmanagedHashTable<TKey, TValue, TValueRef, TMethods> :
    IEnumerable<Ptr<TValue>>, IDisposable,
    IPtrWrapper<UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>>
    where TValue : unmanaged
    where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {

    public static UnmanagedHashTable<TKey, TValue, TValueRef, TMethods> Allocate(int capacity = 16, int buckets = 17) {
        UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* ptr = MemoryUtils.AllocateUninitialized<UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>>();
        ptr->Init(capacity, buckets);
        return new UnmanagedHashTable<TKey, TValue, TValueRef, TMethods>(ptr);
    }

    private readonly UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* _ptr;
    public UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* Pointer => _ptr;

    public bool IsNull => _ptr == null;

    public readonly int Count {
        get {
            ValidateSelf();
            return _ptr->Count;
        }
    }

    private UnmanagedHashTable(UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* ptr) {
        _ptr = ptr;
    }

    public void Dispose() {
        ValidateSelf();
        _ptr->Dispose();
        MemoryUtils.Free(_ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Set(TValueRef valueRef, bool replace) {
        ValidateSelf();
        return _ptr->Set(valueRef, replace);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineSet(TValueRef valueRef, bool replace) {
        ValidateSelf();
        return _ptr->InlineSet(valueRef, replace);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue* Get(TKey key) {
        ValidateSelf();
        return _ptr->Get(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue* InlineGet(TKey key) {
        ValidateSelf();
        return _ptr->InlineGet(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetOrSet(TValueRef replaceValueRef, out TValue* value) {
        ValidateSelf();
        return _ptr->GetOrSet(replaceValueRef, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineGetOrSet(TValueRef replaceValueRef, out TValue* value) {
        ValidateSelf();
        return _ptr->InlineGetOrSet(replaceValueRef, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, TValue* outValue = null) {
        ValidateSelf();
        return _ptr->Remove(key, outValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineRemove(TKey key, TValue* outValue = null) {
        ValidateSelf();
        return _ptr->InlineRemove(key, outValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        ValidateSelf();
        _ptr->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) {
        ValidateSelf();
        return _ptr->ContainsKey(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineContainsKey(TKey key) {
        ValidateSelf();
        return _ptr->InlineContainsKey(key);
    }
    public UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>.UnmanagedEnumerator GetEnumerator() {
        ValidateSelf();
        return new(_ptr);
    }

    IEnumerator<Ptr<TValue>> IEnumerable<Ptr<TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>*(UnmanagedHashTable<TKey, TValue, TValueRef, TMethods> table) => table._ptr;
    public static implicit operator UnmanagedHashTable<TKey, TValue, TValueRef, TMethods>(UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* table) => new(table);

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal void ValidateSelf() {
        if (IsNull) throw new NullReferenceException($"{nameof(UnmanagedHashTable<TKey, TValue, TValueRef, TMethods>)} is null.");
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        ValidateSelf();
        _ptr->DebugValidate();
    }

}