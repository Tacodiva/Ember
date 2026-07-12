
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Ember.Memory;
using Ember.Utils;

#pragma warning disable CS0420

namespace Ember.Collections;

public unsafe interface IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut>
    : IHashTableMethods<TKey, TValue, TValueRef>
    where TKey : allows ref struct
    where TValue : unmanaged
    where TValueRef : allows ref struct
    where TValueOut : unmanaged {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void CopyValueOut(TValue* value, TValueOut* valueOut);

}

public static class UnmanagedInlineConcurrentHashTable {
    public static UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>.RefEnumerable Values<TKey, TValue, TValueRef, TValueOut, TMethods>(this ref UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> @this)
        where TKey : allows ref struct
        where TValue : unmanaged
        where TValueRef : allows ref struct
        where TValueOut : unmanaged
        where TMethods : struct, IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut> {
        return new(ref @this);
    }
}


public unsafe struct UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> : IDisposable
    where TKey : allows ref struct
    where TValue : unmanaged
    where TValueRef : allows ref struct
    where TValueOut : unmanaged
    where TMethods : struct, IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut> {

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static void WaitForEqual<T>(ref T value, T comparand) where T : unmanaged {
        if (EqualityComparer<T>.Default.Equals(value, comparand)) return;
        SpinWait wait = new();
        do {
            wait.SpinOnce();
        } while (!EqualityComparer<T>.Default.Equals(value, comparand));
    }

    private struct Entry {
#if EMBER_SAFETY_CHECKS
        public MemoryGuard MemoryGuard;
#endif

        public TValue Value;
        public Entry* Next;
        public uint HashCode;
        public bool Allocated;
    }

    private struct Bucket {
        public SpinLockValue Lock;
        public Entry Head;
    }

#if EMBER_SAFETY_CHECKS
    private ulong _memoryGuard;
    private ulong _modificationNumber;
#endif

    private AtomicBool _pointerWriteLock;
    private volatile int _pointerAccessCount;

    private volatile int _valueWriteLockCount;
    private volatile int _valueWriteCount;

    private int _count;
    private int _bucketsCapacity;
    private ulong _bucketsCapacityMultiplier;
    private Bucket* _buckets;

    private int _linkedEntryCapacity;
    private int _linkedEntryCount;
    private Entry* _linkedEntries;
    private Entry* _linkedFreeList;

    private int _publicEnumeratorCount;

    public readonly int Count => _count;

    public UnmanagedInlineConcurrentHashTable(int capacity = 16) => Init(capacity);

    public UnmanagedInlineConcurrentHashTable(int capacity, int buckets) => Init(capacity, buckets);

    public void Init(int capacity = 16)
        => Init(capacity, HashUtils.GetPrime(capacity));

    public void Init(int capacity, int buckets) {
        _pointerAccessCount = 0;
        _pointerWriteLock = false;
        _count = 0;
        _bucketsCapacity = buckets;
        _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)buckets);
        _buckets = MemoryUtils.Allocate<Bucket>(buckets);
        _linkedEntryCount = 0;
        _linkedEntryCapacity = capacity;
        _linkedEntries = MemoryUtils.Allocate<Entry>(capacity);
        _linkedFreeList = null;
        _publicEnumeratorCount = 0;
#if EMBER_SAFETY_CHECKS
        _modificationNumber = 0;
        _memoryGuard = 0;
        InitMemoryGuards();
#endif
    }

    public void Dispose() {
        ValidateSelf();
        MemoryUtils.Free(_linkedEntries);
        MemoryUtils.Free(_buckets);
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private void InitMemoryGuards() {
#if EMBER_SAFETY_CHECKS
        _modificationNumber = 0;
        for (int i = 0; i < _bucketsCapacity; i++)
            _buckets[i].Head.MemoryGuard.Init();
        for (int i = 0; i < _linkedEntryCapacity; i++)
            _linkedEntries[i].MemoryGuard.Init();
#endif
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private bool TryEnterGlobalReadLock() {
        Interlocked.Increment(ref _pointerAccessCount);

        if (!_pointerWriteLock) return true;

        Interlocked.Decrement(ref _pointerAccessCount);
        return false;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void EnterPointerReadLock() {
        Interlocked.Increment(ref _pointerAccessCount);
        if (_pointerWriteLock) {
            Interlocked.Decrement(ref _pointerAccessCount);
            SpinWait spinWait = new();
            do {
                spinWait.SpinOnce();
            } while (_pointerWriteLock);
            Interlocked.Increment(ref _pointerAccessCount);
        }
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void ExitPointerReadLock() {
        int value = Interlocked.Decrement(ref _pointerAccessCount);
        Debug.Assert(value >= 0);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void WaitNoPointerWriteLock() {
        WaitForEqual(ref _pointerWriteLock, false);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private bool TryEnterPointerWriteLock() {
        if (!_pointerWriteLock.FalseToTrue())
            return false;

        WaitForEqual(ref _pointerAccessCount, 0);
        return true;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void EnterPointerWriteLock() {
        if (!TryEnterPointerWriteLock()) {
            SpinWait wait = new();
            do {
                wait.SpinOnce();
            } while (!TryEnterPointerWriteLock());
        }
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void ExitPointerWriteLock() {
        Debug.Assert(_pointerWriteLock);
        _pointerWriteLock = false;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void EnterBucketLock(Bucket* bucket) {
        UnmanagedSpinLock.Aquire(&bucket->Lock);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void ExitBucketLock(Bucket* bucket) {
        UnmanagedSpinLock.Release(&bucket->Lock);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void BeginValueWrite() {
        Interlocked.Increment(ref _valueWriteCount);

        while (_valueWriteLockCount != 0) {
            Interlocked.Decrement(ref _valueWriteCount);
            SpinWait wait = new();
            do {
                if (_publicEnumeratorCount != 0)
                    throw new InvalidOperationException("Cannot write to collection while it's being enumerated.");
                wait.SpinOnce();
            } while (_valueWriteLockCount != 0);

            Interlocked.Increment(ref _valueWriteCount);
        }
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void EndValueWrite() {
        int value = Interlocked.Decrement(ref _valueWriteCount);
        Debug.Assert(value >= 0);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void BeginValueWriteLock() {
        EnterPointerReadLock();
        Interlocked.Increment(ref _valueWriteLockCount);
        WaitForEqual(ref _valueWriteCount, 0);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void EndValueWriteLock() {
        int value = Interlocked.Decrement(ref _valueWriteLockCount);
        Debug.Assert(value >= 0);
        ExitPointerReadLock();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetBucketIndex(uint hash) {
        return HashUtils.FastMod(hash & 0x7FFFFFFF, (uint)_bucketsCapacity, _bucketsCapacityMultiplier);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetBucketIndex(Entry* entry) {
        return GetBucketIndex(entry->HashCode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint CalculateHashCode(in TValue key) {
        return (uint)key.GetHashCode();
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void ResizeLinkedEntries() {
        int oldCapacity = _linkedEntryCapacity;
        _linkedEntryCapacity *= 2;

        long freeListOffset = _linkedFreeList - _linkedEntries;
        _linkedEntries = MemoryUtils.Resize(_linkedEntries, oldCapacity, _linkedEntryCapacity);

        Bucket* buckets = _buckets;
        for (int i = 0; i < _bucketsCapacity; i++)
            buckets[i].Head.Next = null;

        for (int i = 0; i < _linkedEntryCount; i++) {
            Entry* entry = &_linkedEntries[i];
            if (entry->Allocated) {
                Entry* bucket = &_buckets[GetBucketIndex(entry->HashCode)].Head;
                entry->Next = bucket->Next;
                bucket->Next = entry;
            }
        }

        InitMemoryGuards();

        if (_linkedFreeList != null)
            _linkedFreeList = freeListOffset + _linkedEntries;
    }

    private Entry* AllocateLinkedEntry() {
        Entry* entry;
        do {
            entry = _linkedFreeList;

            if (entry == null) {
                int linkedEntryCount = Interlocked.Increment(ref _linkedEntryCount);

                if (linkedEntryCount < _linkedEntryCapacity)
                    return &_linkedEntries[linkedEntryCount - 1];

                ExitPointerReadLock();

                while (!TryEnterPointerWriteLock()) {
                    WaitNoPointerWriteLock();
                    if (linkedEntryCount < _linkedEntryCapacity) {
                        EnterPointerReadLock();
                        return &_linkedEntries[linkedEntryCount - 1];
                    }
                }

                if (linkedEntryCount < _linkedEntryCapacity) {
                    ExitPointerWriteLock();
                    EnterPointerReadLock();
                    return &_linkedEntries[linkedEntryCount - 1];
                }

                ResizeLinkedEntries();

                ExitPointerWriteLock();
                EnterPointerReadLock();

                return &_linkedEntries[linkedEntryCount - 1];
            }

        } while (UnsafeInterlocked.CompareExchange(ref _linkedFreeList, entry->Next, entry) != entry);

        return entry;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void FreeLinkedEntry(Entry* entry) {
        entry->Allocated = false;
        do {
            entry->Next = _linkedFreeList;
        } while (UnsafeInterlocked.CompareExchange(ref _linkedFreeList, entry, entry->Next) != entry->Next);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private bool TryResizeBuckets() {
        if (_count < _bucketsCapacity)
            return false;

        while (!TryEnterPointerWriteLock()) {
            WaitNoPointerWriteLock();
            if (_count < _bucketsCapacity)
                return true;
        }

        ResizeBuckets();

        ExitPointerWriteLock();

        return true;
    }

    private void ResizeBuckets() {
        int oldCapacity = _bucketsCapacity;
        int capacity = _bucketsCapacity = HashUtils.ExpandPrime(oldCapacity);
        _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)_bucketsCapacity);

        Bucket* oldBuckets = _buckets;
        Bucket* newBuckets = _buckets = MemoryUtils.Allocate<Bucket>(capacity);

        // Firstly, we try to copy over the entry buckets. Because of the changed number of buckets,
        //  two entry buckets could now map to the same bucket. We deal with this edge case at the end.
        int blockedBucketCount = 0;

        for (int i = 0; i < oldCapacity; i++) {
            Bucket* oldBucket = &oldBuckets[i];
            if (oldBucket->Head.Allocated) {
                Bucket* newBucket = &newBuckets[GetBucketIndex(oldBucket->Head.HashCode)];
                if (!newBucket->Head.Allocated) {
                    // If the new destination bucket hasn't been allocated, we can just copy this bucket
                    //  into the new one.
                    *newBucket = *oldBucket;
                    newBucket->Head.Next = null;
                } else {
                    // A weird optomization, but we store the list of blocked buckets in the 'next' values
                    //  of old buckets we've already checked to avoid allocating a new list for them.
                    oldBuckets[blockedBucketCount++].Head.Next = &oldBucket->Head;
                }
            }
        }

        // Now, copy over the linked entries.
        for (int i = 0; i < _linkedEntryCount; i++) {
            Entry* linkedEntry = &_linkedEntries[i];
            if (linkedEntry->Allocated) {
                Bucket* bucket = &newBuckets[GetBucketIndex(linkedEntry->HashCode)];
                if (bucket->Head.Allocated) {
                    // If the bucket is allocated, we can just add ourselfs to the linked list.
                    linkedEntry->Next = bucket->Head.Next;
                    bucket->Head.Next = linkedEntry;
                } else {
                    // Otherwise, we need to promote this linked entry to a bucket.
                    // Copy it in
                    *bucket = new() { Head = *linkedEntry };
                    bucket->Head.Next = null;
                    // Free the old linked entry
                    FreeLinkedEntry(linkedEntry);
                }
            }
        }

        // Now to deal with those edge cases.
        for (int i = 0; i < blockedBucketCount; i++) {
            Entry* oldBucketEntry = oldBuckets[i].Head.Next;
            Bucket* newBucket = &newBuckets[GetBucketIndex(oldBucketEntry->HashCode)];
            // If this entry bucket is still allocated it means it hasn't been copied and it overlaps
            // with an existing bucket. We need to convert it to a linked bucket.
            // This has to wait until the end because we may need to resize the linked entries array
            // and we can't do that until the loop above makes the linked entries valid again.
            Entry* newLinkedEntry = AllocateLinkedEntry();
            *newLinkedEntry = *oldBucketEntry;
            newLinkedEntry->Next = newBucket->Head.Next;
            newBucket->Head.Next = newLinkedEntry;
        }

        InitMemoryGuards();

        MemoryUtils.Free(oldBuckets);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Set(TValueRef valueRef, bool replace) => InlineSet(valueRef, replace);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineSet(TValueRef valueRef, bool replace) {

        ValidateSelf();
        EnterPointerReadLock();

        try {
            IncrementModificationNumber();

            uint hashCode = TMethods.GetValueRefHashCode(valueRef);
            uint bucketIndex = GetBucketIndex(hashCode);
            Bucket* bucket = &_buckets[bucketIndex];

            EnterBucketLock(bucket);

            try {
                Entry* entry = &bucket->Head;

                if (entry->Allocated) {
                    do {
                        ValidateEntry(entry, true);
                        if (entry->HashCode == hashCode && TMethods.IsValueRefValueEqual(valueRef, &entry->Value)) {
                            if (!replace)
                                return false;

                            BeginValueWrite();

                            TMethods.DereferenceValue(valueRef, &entry->Value);

                            EndValueWrite();
                            return true;
                        }
                        entry = entry->Next;
                    } while (entry != null);

                    BeginValueWrite();
                    entry = AllocateLinkedEntry();
                    entry->Next = bucket->Head.Next;
                    bucket->Head.Next = entry;
                } else {
                    BeginValueWrite();
                    entry->Next = null;
                }

                Interlocked.Increment(ref _count);

                ValidateEntry(entry, false);

                entry->Allocated = true;

                TMethods.DereferenceValue(valueRef, &entry->Value);
                entry->HashCode = hashCode;

                EndValueWrite();
            } finally {
                ExitBucketLock(bucket);
            }

        } finally {
            ExitPointerReadLock();
        }

        TryResizeBuckets();

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGet(TKey key, TValueOut* outValue) => InlineTryGet(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineTryGet(TKey key, TValueOut* outValue) {
        ValidateSelf();
        EnterPointerReadLock();

        try {
            IncrementModificationNumber();

            uint hashCode = TMethods.GetKeyHashCode(key);
            uint bucketIndex = GetBucketIndex(hashCode);
            Bucket* bucket = &_buckets[bucketIndex];

            EnterBucketLock(bucket);

            try {
                Entry* entry = &bucket->Head;

                if (!entry->Allocated)
                    return false;

                do {
                    ValidateEntry(entry, true);

                    if (entry->HashCode == hashCode && TMethods.IsKeyValueEqual(key, &entry->Value)) {
                        if (outValue != null)
                            TMethods.CopyValueOut(&entry->Value, outValue);
                        return true;
                    }

                    entry = entry->Next;
                } while (entry != null);

                return false;
            } finally {
                ExitBucketLock(bucket);
            }
        } finally {
            ExitPointerReadLock();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef valueRef, TValueOut* outValue) => InlineGetOrSet(valueRef, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef valueRef, TValueOut* outValue) {
        ValidateSelf();
        EnterPointerReadLock();

        try {
            IncrementModificationNumber();

            uint hashCode = TMethods.GetValueRefHashCode(valueRef);
            uint bucketIndex = GetBucketIndex(hashCode);

            Bucket* bucket = &_buckets[bucketIndex];
            Entry* entry = &bucket->Head;

            EnterBucketLock(bucket);

            try {
                if (entry->Allocated) {
                    do {
                        ValidateEntry(entry, true);

                        if (entry->HashCode == hashCode && TMethods.IsValueRefValueEqual(valueRef, &entry->Value)) {
                            if (outValue != null)
                                TMethods.CopyValueOut(&entry->Value, outValue);
                            return false;
                        }

                        entry = entry->Next;
                    } while (entry != null);

                    BeginValueWrite();
                    entry = AllocateLinkedEntry();
                    entry->Next = bucket->Head.Next;
                    bucket->Head.Next = entry;
                } else {
                    BeginValueWrite();
                    entry->Next = null;
                }

                Interlocked.Increment(ref _count);

                ValidateEntry(entry, false);

                entry->Allocated = true;

                TMethods.DereferenceValue(valueRef, &entry->Value);
                TMethods.CopyValueOut(&entry->Value, outValue);

                entry->HashCode = hashCode;
                EndValueWrite();

            } finally {
                ExitBucketLock(bucket);
            }

        } finally {
            ExitPointerReadLock();
        }

        TryResizeBuckets();

        return true;
    }

    public delegate void ValueFactory<TArg>(in TArg arg, TValue* dst);


    public bool GetOrAdd<TArg>(TKey key, TValue* outValue, ValueFactory<TArg> factory, in TArg argument) {
        return InlineGetOrAdd<TArg>(key, outValue, factory, argument);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrAdd<TArg>(TKey key, TValue* outValue, ValueFactory<TArg> factory, in TArg argument) {
        ValidateSelf();
        EnterPointerReadLock();

        try {
            IncrementModificationNumber();

            uint hashCode = TMethods.GetKeyHashCode(key);
            uint bucketIndex = GetBucketIndex(hashCode);

            Bucket* bucket = &_buckets[bucketIndex];
            Entry* entry = &bucket->Head;

            EnterBucketLock(bucket);

            try {
                if (entry->Allocated) {
                    do {
                        ValidateEntry(entry, true);

                        if (entry->HashCode == hashCode && TMethods.IsKeyValueEqual(key, &entry->Value)) {
                            *outValue = entry->Value;
                            return false;
                        }

                        entry = entry->Next;
                    } while (entry != null);

                    BeginValueWrite();
                    entry = AllocateLinkedEntry();
                    entry->Next = bucket->Head.Next;
                    bucket->Head.Next = entry;
                } else {
                    BeginValueWrite();
                    entry->Next = null;
                }

                Interlocked.Increment(ref _count);

                ValidateEntry(entry, false);

                entry->Allocated = true;

                factory(argument, outValue);

                entry->Value = *outValue;
                entry->HashCode = hashCode;
                EndValueWrite();

            } finally {
                ExitBucketLock(bucket);
            }

        } finally {
            ExitPointerReadLock();
        }

        TryResizeBuckets();

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, TValueOut* outValue = null) => InlineRemove(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, TValueOut* outValue = null) {
        ValidateSelf();
        EnterPointerReadLock();

        try {
            IncrementModificationNumber();

            uint hashCode = TMethods.GetKeyHashCode(key);
            uint bucketIndex = GetBucketIndex(hashCode);
            Bucket* bucket = &_buckets[bucketIndex];

            EnterBucketLock(bucket);

            try {
                Entry* entry = &bucket->Head;
                Entry* beforeEntry = null;

                if (entry->Allocated) {
                    do {
                        ValidateEntry(entry, true);

                        if (entry->HashCode == hashCode && TMethods.IsKeyValueEqual(key, &entry->Value)) {

                            if (outValue != null)
                                TMethods.CopyValueOut(&entry->Value, outValue);

                            BeginValueWrite();

                            Interlocked.Decrement(ref _count);

                            // Is this a bucket entry?
                            if (beforeEntry == null) {
                                // If so, do we have a link to another entry?
                                if (entry->Next == null) {
                                    // If we have no linked entry, this bucket now has no elements.
                                    entry->Allocated = false;
                                } else {
                                    // If we have a linked entry, we need to promote it to a bucket entry. 
                                    // Copy it into the right place
                                    Entry* linkedEntry = entry->Next;
                                    *entry = *linkedEntry;
                                    // Free the old linked entry
                                    FreeLinkedEntry(linkedEntry);
                                }
                            } else {
                                // This is a linked entry.
                                // Remove it from the linked list and free it
                                beforeEntry->Next = entry->Next;
                                FreeLinkedEntry(entry);
                            }

                            EndValueWrite();

                            return true;
                        }

                        beforeEntry = entry;
                        entry = entry->Next;
                    } while (entry != null);
                }

                return false;
            } finally {
                ExitBucketLock(bucket);
            }
        } finally {
            ExitPointerReadLock();
        }
    }

    public void Clear() {
        ValidateSelf();

        EnterPointerWriteLock();

        try {
            IncrementModificationNumber();

            _count = 0;
            _linkedEntryCount = 0;
            _linkedFreeList = null;
            MemoryUtils.Fill(_buckets, _bucketsCapacity);
            MemoryUtils.Fill(_linkedEntries, _linkedEntryCapacity);

            InitMemoryGuards();
        } finally {
            ExitPointerWriteLock();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ContainsKey(TKey key) => InlineContainsKey(key);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineContainsKey(TKey key) {
        return InlineTryGet(key, null);
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (_memoryGuard != 0)
            throw new InvalidOperationException($"Memory guard '{nameof(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>)}.{nameof(_memoryGuard)}' was corrupted to 0x{_memoryGuard:X}");
        MemoryUtils.ValidateAllocation(_buckets, $"{nameof(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>)}.{nameof(_buckets)}", _bucketsCapacity);
        MemoryUtils.ValidateAllocation(_linkedEntries, $"{nameof(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>)}.{nameof(_linkedEntries)}", _linkedEntryCapacity);
#endif
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private static void ValidateEntry(Entry* entry, bool expectAllocated) {
#if EMBER_SAFETY_CHECKS
        if (entry == null) throw new NullReferenceException(nameof(entry));
        entry->MemoryGuard.Validate($"{nameof(entry)}->{nameof(Entry.MemoryGuard)}");
        if (expectAllocated != entry->Allocated)
            throw new ArgumentException($"{nameof(entry)} did not have the expected allocation state of {expectAllocated}.");
#endif
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Conditional("EMBER_SAFETY_CHECKS")]
    private void IncrementModificationNumber() {
#if EMBER_SAFETY_CHECKS
        Interlocked.Increment(ref _modificationNumber);
#endif
    }

    public readonly struct UnmanagedEnumerable : IEnumerable<Ptr<TValue>> {
        public readonly UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* Table;
        public readonly bool IsInternal;

        internal UnmanagedEnumerable(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* table, bool isInternal) {
            Table = table;
            IsInternal = isInternal;
        }

        public UnmanagedEnumerable(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* table) :
            this(table, false) { }

        public UnmanagedEnumerator GetEnumerator() => new(Table, IsInternal);
        IEnumerator<Ptr<TValue>> IEnumerable<Ptr<TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct UnmanagedEnumerator : IEnumerator<Ptr<TValue>> {
        private int _bucket;
        private Entry* _entry;
        private readonly UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* _table;
        private readonly bool _isInternal;

#if EMBER_SAFETY_CHECKS
        private readonly ulong _modificationNumber;
#endif

        public Ptr<TValue> Current {
            get {
                ValidateEntry(_entry, true);
                return &_entry->Value;
            }
        }
        object IEnumerator.Current => Current;

        internal UnmanagedEnumerator(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* tbl, bool isInternal) {
            _table = tbl;
            _table->ValidateSelf();
            _isInternal = isInternal;
#if EMBER_SAFETY_CHECKS
            _modificationNumber = _table->_modificationNumber;
#endif
            if (!_isInternal)
                Interlocked.Increment(ref _table->_publicEnumeratorCount);
            _table->BeginValueWriteLock();

            Reset();
        }

        public UnmanagedEnumerator(UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>* tbl) :
            this(tbl, false) { }

        public bool MoveNext() {
            _table->ValidateSelf();

#if EMBER_SAFETY_CHECKS
            if (_modificationNumber != _table->_modificationNumber)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
#endif

            if (_bucket == _table->_bucketsCapacity) return false;
            if (_bucket == -1 || _entry->Next == null) {
                while (++_bucket < _table->_bucketsCapacity && !(_entry = &_table->_buckets[_bucket].Head)->Allocated) ;
                return _bucket != _table->_bucketsCapacity;
            } else {
                _entry = _entry->Next;
                return true;
            }
        }

        public void Reset() {
            _bucket = -1;
        }

        public readonly void Dispose() {
            _table->EndValueWriteLock();
            if (!_isInternal) {
                int value = Interlocked.Decrement(ref _table->_publicEnumeratorCount);
                Debug.Assert(value >= 0);
            }
        }
    }

    public readonly ref struct RefEnumerable {
        public readonly ref UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> Table;

        internal RefEnumerable(ref UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> table) {
            Table = ref table;
        }

        public RefEnumerator GetEnumerator() => new(ref Table);
    }

    public ref struct RefEnumerator : IEnumerator<Ptr<TValue>> {
        private int _bucket;
        private Entry* _entry;
        private readonly ref UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> _table;

#if EMBER_SAFETY_CHECKS
        private readonly ulong _modificationNumber;
#endif

        public Ptr<TValue> Current {
            get {
                ValidateEntry(_entry, true);
                return &_entry->Value;
            }
        }
        object IEnumerator.Current => Current;

        internal RefEnumerator(ref UnmanagedInlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> tbl) {
            _table = ref tbl;
            _table.ValidateSelf();
#if EMBER_SAFETY_CHECKS
            _modificationNumber = _table._modificationNumber;
#endif
            Interlocked.Increment(ref _table._publicEnumeratorCount);
            _table.BeginValueWriteLock();

            Reset();
        }

        public bool MoveNext() {
            _table.ValidateSelf();

#if EMBER_SAFETY_CHECKS
            if (_modificationNumber != _table._modificationNumber)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
#endif

            if (_bucket == _table._bucketsCapacity) return false;
            if (_bucket == -1 || _entry->Next == null) {
                while (++_bucket < _table._bucketsCapacity && !(_entry = &_table._buckets[_bucket].Head)->Allocated) ;
                return _bucket != _table._bucketsCapacity;
            } else {
                _entry = _entry->Next;
                return true;
            }
        }

        public void Reset() {
            _bucket = -1;
        }

        public readonly void Dispose() {
            _table.EndValueWriteLock();
            int value = Interlocked.Decrement(ref _table._publicEnumeratorCount);
            Debug.Assert(value >= 0);
        }
    }


    [Conditional("DEBUG")]
    public void DebugValidate() {
#if DEBUG
        bool error = false;

        ValidateSelf();

        void Error(string msg) {
            Console.WriteLine($"Validation Error: {msg}");
            error |= true;
        }

        bool[] entryMap = new bool[_linkedEntryCapacity];
        HashSet<TValue> foundValues = [];
        HashSet<Ptr<Entry>> visitedEntries = [];

        int expectedEntryCount = 0;
        int bucketCount = 0;
        for (int i = 0; i < _bucketsCapacity; i++) {
            Entry* bucketEntry = &_buckets[i].Head;
            Entry* entry = bucketEntry;
            if (entry->Allocated) {
                ++bucketCount;
                int perBucketEntryCount = 0;
                int entryIndex = -1;
                do {
                    if (!visitedEntries.Add(entry)) {
                        Error($"Entry visited twice.");
                        break;
                    }
                    ++perBucketEntryCount;
                    ++expectedEntryCount;

                    if (TMethods.GetValueHashCode(&entry->Value) != entry->HashCode) {
                        Error($"Wrong hash code for value {entry->Value}");
                    } else if (GetBucketIndex(entry) != i) {
                        Error($"Wrong bucket {i} for value {entry->Value} (expected {GetBucketIndex(entry)})");
                    }

                    if (entry == bucketEntry) {
                        foundValues.Add(entry->Value);
                        entry = entry->Next;
                        continue;
                    } else {
                        entryIndex = (int)((nint)entry - (nint)_linkedEntries) / sizeof(Entry);
                        if (entryIndex < 0 || entryIndex >= _linkedEntryCapacity) {
                            Error($"Hit invalid entry index in bucket {i} at {entryIndex}");
                        } else if (entryMap[entryIndex]) {
                            Error($"Duplicate entry linkage in bucket {i} at {entryIndex}");
                        } else if (foundValues.Contains(entry->Value)) {
                            Error($"Duplicate value {entry->Value} at {entryIndex}");
                        } else if (!entry->Allocated) {
                            Error($"Unallocated entry in bucket {i} at {entryIndex}");
                        } else {
                            foundValues.Add(entry->Value);
                            entryMap[entryIndex] = true;
                            entry = entry->Next;
                            continue;
                        }
                    }
                    break;
                } while (entry != null);
            }
        }
        if (!error && expectedEntryCount != _count) {
            Error($"Total entry count was {_count} but found {expectedEntryCount}.");
        }

        int freeListLength = 0;
        {
            Entry* entry = _linkedFreeList;
            while (entry != null) {
                ++freeListLength;
                int entryIndex = (int)((nint)entry - (nint)_linkedEntries) / sizeof(Entry);
                if (entryIndex < 0 || entryIndex >= _linkedEntryCapacity) {
                    Error($"Hit invalid entry index in free list {entryIndex}");
                } else if (entry->Allocated) {
                    Error($"Allocated entry in free list {entryIndex}");
                } else if (entryMap[entryIndex]) {
                    Error($"Accesiable entry in free list {entryIndex}");
                }
                entry = entry->Next;
            }
        }
        int expectedLinkedEntryCount = expectedEntryCount + freeListLength - bucketCount;
        if (!error && expectedLinkedEntryCount != _linkedEntryCount) {
            Error($"Linked entry count was {_linkedEntryCount} but found {expectedLinkedEntryCount}.");
        }

        for (int i = 0; i < _linkedEntryCapacity; i++) {
            if (i < expectedLinkedEntryCount) {
                if (_linkedEntries[i].Allocated != entryMap[i]) {
                    Error($"Invalid tracked allocation state. {i} is {_linkedEntries[i].Allocated}.");
                }
            } else if (_linkedEntries[i].Allocated) {
                Error($"Entry allocated past linked count!");
            }
        }
        if (error)
            throw new Exception("UnmanagedConcurrentHashTable validation error/s.");
#endif
    }
}