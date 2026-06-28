
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Collections;

internal unsafe readonly struct UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TMethods>
    (UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>* ptr) :
    IDisposable, IEnumerable<Ptr<TValue>>,
    IPtrWrapper<UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>>
    where TValue : unmanaged
    where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {

    public static UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TMethods> Allocate(int capacity = 16, int buckets = 17) {
        UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>* table = MemoryUtils.AllocateUninitialized<UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>>();
        table->Init(capacity, buckets);
        return new UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TMethods>(table);
    }

    private readonly UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>* _ptr = ptr;

    public bool IsNull => _ptr == null;
    public UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>* Pointer => _ptr;

    public readonly int Count {
        get {
            ValidateSelf();
            return _ptr->Count;
        }
    }

    public void Dispose() {
        ValidateSelf();
        _ptr->Dispose();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Set(TValueRef valueRef, bool replace) => InlineSet(valueRef, replace);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineSet(TValueRef valueRef, bool replace) {
        ValidateSelf();
        return _ptr->InlineSet(valueRef, replace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGet(TKey key, TValue* outValue) => InlineTryGet(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineTryGet(TKey key, TValue* outValue) {
        ValidateSelf();
        return _ptr->InlineTryGet(key, outValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef valueRef, TValue* outValue) => InlineGetOrSet(valueRef, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef valueRef, TValue* outValue) {
        ValidateSelf();
        return _ptr->InlineGetOrSet(valueRef, outValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, TValue* outValue = null) => InlineRemove(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, TValue* outValue = null) {
        ValidateSelf();
        return _ptr->InlineRemove(key, outValue);
    }

    public void Clear() {
        ValidateSelf();
        _ptr->Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ContainsKey(TKey key) => InlineContainsKey(key);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineContainsKey(TKey key) {
        ValidateSelf();
        return _ptr->InlineContainsKey(key);
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException($"{nameof(UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TMethods>)} is null.");
#endif
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>.Enumerator GetEnumerator() {
        ValidateSelf();
        return new(_ptr);
    }

    IEnumerator<Ptr<TValue>> IEnumerable<Ptr<TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TMethods>.Enumerable GetEnumerable(bool isInternal) {
        ValidateSelf();
        return new(_ptr, isInternal);
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _ptr->DebugValidate();
    }
}