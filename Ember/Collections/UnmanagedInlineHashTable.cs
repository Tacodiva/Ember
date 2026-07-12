
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Logging;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;

public static class UnmanagedInlineHashTable {
    public static UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>.RefEnumerable Values<TKey, TValue, TValueRef, TMethods>(this ref UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> @this)
        where TKey : allows ref struct
        where TValue : unmanaged
        where TValueRef : allows ref struct
        where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {
        return new(ref @this);
    }
}

public unsafe struct UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> : IDisposable
    where TKey : allows ref struct
    where TValue : unmanaged
    where TValueRef : allows ref struct
    where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {

    private struct Entry {
#if EMBER_SAFETY_CHECKS
        public MemoryGuard MemoryGuard;
#endif

        public TValue Value;
        public Entry* Next;
        public uint HashCode;
        public bool Allocated;
    }

#if EMBER_SAFETY_CHECKS
    private ulong _memoryGuard;
    private ulong _modificationNumber;
#endif

    private int _count;
    public int Count {
        get {
            ValidateSelf();
            return _count;
        }
    }

    private int _bucketsCapacity;
    private ulong _bucketsCapacityMultiplier;
    private Entry* _buckets;

    private int _linkedEntryCapacity;
    private int _linkedEntryCount;
    private Entry* _linkedEntries;
    private Entry* _linkedFreeList;

    public UnmanagedInlineHashTable(int capacity = 16) => Init(capacity);

    public UnmanagedInlineHashTable(int capacity, int buckets) => Init(capacity, buckets);

    public void Init(int capacity = 16)
        => Init(capacity, HashUtils.GetPrime(capacity));

    public void Init(int capacity, int buckets) {
        _count = 0;
        _bucketsCapacity = buckets;
        _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)buckets);
        _buckets = MemoryUtils.Allocate<Entry>(buckets);
        _linkedEntryCount = 0;
        _linkedEntryCapacity = capacity;
        _linkedEntries = MemoryUtils.Allocate<Entry>(capacity);
        _linkedFreeList = null;

#if EMBER_SAFETY_CHECKS
        _modificationNumber = 0;
        _memoryGuard = 0;
        InitMemoryGuards();
#endif
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private readonly void InitMemoryGuards() {
#if EMBER_SAFETY_CHECKS
        for (int i = 0; i < _bucketsCapacity; i++)
            _buckets[i].MemoryGuard.Init();
        for (int i = 0; i < _linkedEntryCapacity; i++)
            _linkedEntries[i].MemoryGuard.Init();
#endif
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
        int capacity = _linkedEntryCapacity = oldCapacity * 2;
        long freeListOffset = _linkedFreeList - _linkedEntries;
        _linkedEntries = MemoryUtils.Resize(_linkedEntries, oldCapacity, capacity);

        Entry* buckets = _buckets;
        for (int i = 0; i < _bucketsCapacity; i++)
            buckets[i].Next = null;

        for (int i = 0; i < _linkedEntryCount; i++) {
            Entry* entry = &_linkedEntries[i];
            if (entry->Allocated) {
                Entry* bucket = &_buckets[GetBucketIndex(entry->HashCode)];
                entry->Next = bucket->Next;
                bucket->Next = entry;
            }
        }

        InitMemoryGuards();

        if (_linkedFreeList != null)
            _linkedFreeList = freeListOffset + _linkedEntries;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private Entry* AllocateLinkedEntry() {
        Entry* entry;
        if (_linkedFreeList == null) {
            int linkedEntryCount = _linkedEntryCount;
            if (linkedEntryCount == _linkedEntryCapacity)
                ResizeLinkedEntries();
            entry = &_linkedEntries[linkedEntryCount];
            ++_linkedEntryCount;
        } else {
            entry = _linkedFreeList;
            _linkedFreeList = entry->Next;
        }
        return entry;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void FreeLinkedEntry(Entry* entry) {
        entry->Allocated = false;
        entry->Next = _linkedFreeList;
        _linkedFreeList = entry;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void ResizeBuckets() {
        int oldCapacity = _bucketsCapacity;
        int capacity = _bucketsCapacity = HashUtils.ExpandPrime(oldCapacity);
        _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)_bucketsCapacity);

        Entry* oldBuckets = _buckets;
        Entry* newBuckets = _buckets = MemoryUtils.Allocate<Entry>((int)capacity);

        // Firstly, we try to copy over the entry buckets. Because of the changed number of buckets,
        //  two entry buckets could now map to the same bucket. We deal with this edge case at the end.
        int blockedBucketCount = 0;

        for (int i = 0; i < oldCapacity; i++) {
            Entry* oldBucket = &oldBuckets[i];
            if (oldBucket->Allocated) {
                Entry* newBucket = &newBuckets[GetBucketIndex(oldBucket->HashCode)];
                if (!newBucket->Allocated) {
                    // If the new destination bucket hasn't been allocated, we can just copy this bucket
                    //  into the new one.
                    *newBucket = *oldBucket;
                    newBucket->Next = null;
                } else {
                    // A weird optomization, but we store the list of blocked buckets in the 'next' values
                    //  of old buckets we've already checked to avoid allocating a new list for them.
                    oldBuckets[blockedBucketCount++].Next = oldBucket;
                }
            }
        }

        // Now, copy over the linked entries.
        for (int i = 0; i < _linkedEntryCount; i++) {
            Entry* linkedEntry = &_linkedEntries[i];
            if (linkedEntry->Allocated) {
                Entry* bucket = &newBuckets[GetBucketIndex(linkedEntry->HashCode)];
                if (bucket->Allocated) {
                    // If the bucket is allocated, we can just add ourselfs to the linked list.
                    linkedEntry->Next = bucket->Next;
                    bucket->Next = linkedEntry;
                } else {
                    // Otherwise, we need to promote this linked entry to a bucket.
                    // Copy it in
                    *bucket = *linkedEntry;
                    bucket->Next = null;
                    // Free the old linked entry
                    FreeLinkedEntry(linkedEntry);
                }
            }
        }

        // Now to deal with those edge cases.
        for (int i = 0; i < blockedBucketCount; i++) {
            Entry* oldBucket = oldBuckets[i].Next;
            Entry* newBucket = &newBuckets[GetBucketIndex(oldBucket->HashCode)];
            // If this entry bucket is still allocated it means it hasn't been copied and it overlaps
            // with an existing bucket. We need to convert it to a linked bucket.
            // This has to wait until the end because we may need to resize the linked entries array
            // and we can't do that until the loop above makes the linked entries valid again.
            Entry* newLinkedEntry = AllocateLinkedEntry();
            *newLinkedEntry = *oldBucket;
            newLinkedEntry->Next = newBucket->Next;
            newBucket->Next = newLinkedEntry;
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
        IncrementModificationNumber();

        uint hashCode = TMethods.GetValueRefHashCode(valueRef);
        uint bucketIndex = GetBucketIndex(hashCode);
        Entry* bucketEntry = &_buckets[bucketIndex];

        Entry* entry = bucketEntry;

        if (bucketEntry->Allocated) {
            do {
                ValidateEntry(entry, true);
                if (entry->HashCode == hashCode && TMethods.IsValueRefValueEqual(valueRef, &entry->Value)) {
                    if (!replace)
                        return false;
                    TMethods.DereferenceValue(valueRef, &entry->Value);
                    return true;
                }
                entry = entry->Next;
            } while (entry != null);
            entry = AllocateLinkedEntry();
            entry->Next = bucketEntry->Next;
            bucketEntry->Next = entry;
        } else {
            entry->Next = null;
        }

        ++_count;

        ValidateEntry(entry, false);

        entry->Allocated = true;

        TMethods.DereferenceValue(valueRef, &entry->Value);

        entry->HashCode = hashCode;

        if (_count >= _bucketsCapacity)
            ResizeBuckets();

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public TValue* Get(TKey key) => InlineGet(key);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public TValue* InlineGet(TKey key) {
        ValidateSelf();
        IncrementModificationNumber();

        uint hashCode = TMethods.GetKeyHashCode(key);
        uint bucketIndex = GetBucketIndex(hashCode);
        Entry* entry = &_buckets[bucketIndex];

        if (!entry->Allocated)
            return null;

        do {
            ValidateEntry(entry, true);

            if (entry->HashCode == hashCode && TMethods.IsKeyValueEqual(key, &entry->Value))
                return &entry->Value;

            entry = entry->Next;
        } while (entry != null);

        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef replaceValueRef, out TValue* value)
        => InlineGetOrSet(replaceValueRef, out value);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef replaceValueRef, out TValue* value) {
        ValidateSelf();
        IncrementModificationNumber();

        uint hashCode = TMethods.GetValueRefHashCode(replaceValueRef);
        uint bucketIndex = GetBucketIndex(hashCode);

        Entry* bucketEntry = &_buckets[bucketIndex];
        Entry* entry = bucketEntry;

        if (entry->Allocated) {
            do {
                ValidateEntry(entry, true);

                if (entry->HashCode == hashCode && TMethods.IsValueRefValueEqual(replaceValueRef, &entry->Value)) {
                    value = &entry->Value;
                    return false;
                }

                entry = entry->Next;
            } while (entry != null);

            entry = AllocateLinkedEntry();
            entry->Next = bucketEntry->Next;
            bucketEntry->Next = entry;
        } else {
            entry->Next = null;
        }

        ++_count;

        ValidateEntry(entry, false);

        entry->Allocated = true;

        TMethods.DereferenceValue(replaceValueRef, &entry->Value);
        entry->HashCode = hashCode;

        if (_count >= _bucketsCapacity)
            ResizeBuckets();

        value = &entry->Value;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, TValue* outValue = null) => InlineRemove(key, outValue);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, TValue* outValue = null) {
        ValidateSelf();
        IncrementModificationNumber();

        uint hashCode = TMethods.GetKeyHashCode(key);
        uint bucketIndex = GetBucketIndex(hashCode);
        Entry* entry = &_buckets[bucketIndex];
        Entry* beforeEntry = null;

        if (entry->Allocated) {
            do {
                ValidateEntry(entry, true);

                if (entry->HashCode == hashCode && TMethods.IsKeyValueEqual(key, &entry->Value)) {
                    --_count;

                    if (outValue != null)
                        *outValue = entry->Value;

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

                    return true;
                }

                beforeEntry = entry;
                entry = entry->Next;
            } while (entry != null);
        }

        return false;
    }

    public void Clear() {
        ValidateSelf();
        IncrementModificationNumber();

        _count = 0;
        _linkedEntryCount = 0;
        _linkedFreeList = null;
        MemoryUtils.Fill(_buckets, _bucketsCapacity);
        MemoryUtils.Fill(_linkedEntries, _linkedEntryCapacity);

        InitMemoryGuards();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ContainsKey(TKey key) => InlineContainsKey(key);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineContainsKey(TKey key) {
        return InlineGet(key) != null;
    }

    public readonly void Dispose() {
        ValidateSelf();
        MemoryUtils.Free(_linkedEntries);
        MemoryUtils.Free(_buckets);
    }

    public readonly ref struct RefEnumerable {
        private readonly ref UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> _table;

        public RefEnumerable(ref UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> tbl) {
            _table = ref tbl;
        }

        public RefEnumerator GetEnumerator() => new(ref _table);
    }

    public ref struct RefEnumerator : IEnumerator<Ptr<TValue>> {
        private int _bucket;
        private Entry* _entry;
        private readonly ref UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> _table;

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

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public RefEnumerator(ref UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods> tbl) {
            _table = ref tbl;
            _table.ValidateSelf();
#if EMBER_SAFETY_CHECKS
            _modificationNumber = _table._modificationNumber;
#endif
            Reset();
        }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool MoveNext() {
            _table.ValidateSelf();

#if EMBER_SAFETY_CHECKS
            if (_modificationNumber != _table._modificationNumber)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
#endif

            if (_bucket == _table._bucketsCapacity) return false;
            if (_bucket == -1 || _entry->Next == null) {
                while (++_bucket < _table._bucketsCapacity && !(_entry = &_table._buckets[_bucket])->Allocated) ;
                return _bucket != _table._bucketsCapacity;
            } else {
                _entry = _entry->Next;
                return true;
            }
        }

        public void Reset() {
            _bucket = -1;
        }

        public readonly void Dispose() { }
    }

    public struct UnmanagedEnumerator : IEnumerator<Ptr<TValue>> {
        private int _bucket;
        private Entry* _entry;
        private readonly UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* _table;

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

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public UnmanagedEnumerator(in UnmanagedInlineHashTable<TKey, TValue, TValueRef, TMethods>* tbl) {
            _table = tbl;
            _table->ValidateSelf();
#if EMBER_SAFETY_CHECKS
            _modificationNumber = _table->_modificationNumber;
#endif
            Reset();
        }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool MoveNext() {
            _table->ValidateSelf();

#if EMBER_SAFETY_CHECKS
            if (_modificationNumber != _table->_modificationNumber)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
#endif

            if (_bucket == _table->_bucketsCapacity) return false;
            if (_bucket == -1 || _entry->Next == null) {
                while (++_bucket < _table->_bucketsCapacity && !(_entry = &_table->_buckets[_bucket])->Allocated) ;
                return _bucket != _table->_bucketsCapacity;
            } else {
                _entry = _entry->Next;
                return true;
            }
        }

        public void Reset() {
            _bucket = -1;
        }

        public readonly void Dispose() { }
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (_linkedEntryCount > _linkedEntryCapacity) throw new InvalidOperationException($"{nameof(_linkedEntryCount)} > {nameof(_linkedEntryCapacity)}");
        if (_linkedEntryCount < 0) throw new InvalidOperationException($"{nameof(_linkedEntryCount)} < 0");
        if (_linkedEntryCapacity < 0) throw new InvalidOperationException($"{nameof(_linkedEntryCapacity)} < 0");
        if (_bucketsCapacity < 0) throw new InvalidOperationException($"{nameof(_bucketsCapacity)} < 0");
        if (_count < 0) throw new InvalidOperationException($"{nameof(_count)} < 0");
        if (_bucketsCapacity != 0) MemoryUtils.ValidateAllocation<Entry>(_buckets, $"{nameof(_buckets)}", _bucketsCapacity);
        if (_linkedEntryCapacity != 0) MemoryUtils.ValidateAllocation<Entry>(_linkedEntries, $"{nameof(_linkedEntries)}", _linkedEntryCapacity);
        if (_memoryGuard != 0)
            throw new InvalidOperationException($"Memory guard '{nameof(_memoryGuard)}' was corrupted to 0x{_memoryGuard:X}");
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
        ++_modificationNumber;
#endif
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
            Entry* bucketEntry = &_buckets[i];
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
            throw new Exception("UnmanagedHashTable validation error/s.");
#endif
    }
}