
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Collections;

internal unsafe readonly struct UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>
    (UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* ptr) :
    IDisposable, IEnumerable<Ptr<TValue>>,
    IPtrWrapper<UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>>
    where TKey : allows ref struct
    where TValue : unmanaged
    where TValueRef : allows ref struct
    where TValueOut : unmanaged
    where TMethods : struct, IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut> {

    public static UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> Allocate(int capacity = 16, int buckets = 17) {
        UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* table = MemoryUtils.AllocateUninitialized<UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>>();
        table->Init(capacity, buckets);
        return new UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>(table);
    }

    private readonly UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* _ptr = ptr;

    public bool IsNull => _ptr == null;
    public UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* Pointer => _ptr;

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
    public bool TryGet(TKey key, TValueOut* outValue) => InlineTryGet(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineTryGet(TKey key, TValueOut* outValue) {
        ValidateSelf();
        return _ptr->InlineTryGet(key, outValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef valueRef, TValueOut* outValue) => InlineGetOrSet(valueRef, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef valueRef, TValueOut* outValue) {
        ValidateSelf();
        return _ptr->InlineGetOrSet(valueRef, outValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, TValueOut* outValue = null) => InlineRemove(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, TValueOut* outValue = null) {
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
        if (IsNull) throw new NullReferenceException($"{nameof(UnmanagedConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>)} is null.");
#endif
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>.UnmanagedEnumerator GetEnumerator() {
        ValidateSelf();
        return new(_ptr);
    }

    IEnumerator<Ptr<TValue>> IEnumerable<Ptr<TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>.UnmanagedEnumerable GetEnumerable(bool isInternal) {
        ValidateSelf();
        return new(_ptr, isInternal);
    }

    [Conditional("DEBUG")]
    public void DebugValidate() {
        _ptr->DebugValidate();
    }
}