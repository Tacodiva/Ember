
#if !DEBUG
#define EMBER_INLINE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Logging;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;

public static class InlineHashTable {
    public static InlineHashTable<TKey, TValue, TValueRef, TMethods>.RefEnumerable Values<TKey, TValue, TValueRef, TMethods>(this ref InlineHashTable<TKey, TValue, TValueRef, TMethods> @this)
        where TKey : allows ref struct
        where TValueRef : allows ref struct
        where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {
        return new(ref @this);
    }
}

public struct InlineHashTable<TKey, TValue, TValueRef, TMethods>
    where TKey : allows ref struct
    where TValueRef : allows ref struct
    where TMethods : struct, IHashTableMethods<TKey, TValue, TValueRef> {

    private enum EntryStatus {
        Free = 0,
        Allocated,
        // A temporary status used during a resize
        Moved
    }

    private struct Entry {
        public TValue Value;
        public int NextIndex; // -1 if no next
        public int HashCode;
        public EntryStatus Status;
    }

#if EMBER_SAFETY_CHECKS
    private ulong _modificationNumber;
#endif

    private int _count;
    public int Count {
        get {
            ValidateSelf();
            return _count;
        }
    }

    // This should always equal _buckets.Length. Calculating a bucket index from a hash requires knowing this,
    //   and storing inline here in the struct can save us a cache miss. 
    private int _bucketsLength;
    private ulong _bucketsCapacityMultiplier;
    private Entry[] _buckets;

    private Entry[] _linkedEntries;
    private int _linkedEntryCount;
    private int _linkedFreeListHeadIndex; // -1 if no free list

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public InlineHashTable(int capacity = 16) => Init(capacity);
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void Init(int capacity = 16) {
        try {
            _count = 0;
            _buckets = new Entry[HashUtils.GetPrime(capacity)];
            _bucketsLength = _buckets.Length;
            _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)_bucketsLength);
            _linkedEntryCount = 0;
            _linkedEntries = new Entry[capacity];
            _linkedFreeListHeadIndex = -1;

#if EMBER_SAFETY_CHECKS
            _modificationNumber = 0;
#endif
        } catch {
            this = default;
            throw;
        }
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private readonly int GetBucketIndex(int hash) {
        return (int)HashUtils.FastMod((uint)hash & 0x7FFFFFFF, (uint)_bucketsLength, _bucketsCapacityMultiplier);
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private readonly ref Entry GetBucket(int hash) {
        return ref _buckets.UnsafeGetReference(GetBucketIndex(hash));
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private readonly ref Entry GetLinkedEntry(int index) {
        Debug.Assert(index >= 0 && index < _linkedEntries.Length);
        return ref _linkedEntries.UnsafeGetReference(index);
    }

    private void ResizeLinkedEntries() {
        int oldCapacity = _linkedEntries.Length;
        int capacity = oldCapacity * 2;

        Array.Resize(ref _linkedEntries, capacity);
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private ref Entry AllocateLinkedEntry(out int index) {
        if (_linkedFreeListHeadIndex == -1) {
            if (_linkedEntryCount == _linkedEntries.Length)
                ResizeLinkedEntries();

            index = _linkedEntryCount++;
            return ref GetLinkedEntry(index);
        } else {
            index = _linkedFreeListHeadIndex;
            ref Entry entry = ref GetLinkedEntry(index);
            _linkedFreeListHeadIndex = entry.NextIndex;
            return ref entry;
        }
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void FreeEntry(int entryIndex, ref Entry entry) {
        if (entryIndex == -1) FreeBucket(ref entry);
        else FreeLinkedEntry(entryIndex, ref entry);
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private readonly void FreeBucket(ref Entry entry) {
        Debug.Assert(_buckets.GetReferenceIndex(ref entry) != -1);
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            entry.Value = default!;
        entry.Status = EntryStatus.Free;
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void FreeLinkedEntry(int entryIndex, ref Entry entry) {
        Debug.Assert(_linkedEntries.GetReferenceIndex(ref entry) == entryIndex);
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            entry.Value = default!;
        entry.Status = EntryStatus.Free;
        entry.NextIndex = _linkedFreeListHeadIndex;
        _linkedFreeListHeadIndex = entryIndex;
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void TryResizeBuckets() {
        if (_count >= _bucketsLength)
            ResizeBuckets();
    }

    private void ResizeBuckets() {
        Entry[] oldBuckets = _buckets;
        int oldBucketsLength = _bucketsLength;
        ulong oldCapacityMultiplier = _bucketsCapacityMultiplier;

        try {
            _bucketsLength = HashUtils.ExpandPrime(oldBucketsLength);
            _buckets = new Entry[_bucketsLength];
            _bucketsCapacityMultiplier = HashUtils.GetFastModMultiplier((uint)_bucketsLength);


            // Firstly, we try to copy over the bucket entries.
            for (int oldBucketIndex = 0; oldBucketIndex < oldBucketsLength; oldBucketIndex++) {
                ref Entry oldBucket = ref oldBuckets[oldBucketIndex];
                if (oldBucket.Status == EntryStatus.Allocated) {
                    ref Entry newBucket = ref GetBucket(oldBucket.HashCode);
                    if (newBucket.Status == EntryStatus.Free) {
                        // If the new destination bucket hasn't been allocated, we can just copy this bucket
                        //  into the new one.
                        newBucket = oldBucket;
                        newBucket.NextIndex = -1;
                    } else {
                        // If the new destination is allocated we need to add a new linked entry to that bucket
                        ref Entry newLinkedEntry = ref AllocateLinkedEntry(out int newLinkedEntryIndex);
                        newLinkedEntry = oldBucket;
                        newLinkedEntry.NextIndex = newBucket.NextIndex;
                        // We set the status to moved so we don't try to move it again later
                        newLinkedEntry.Status = EntryStatus.Moved;
                        newBucket.NextIndex = newLinkedEntryIndex;
                    }
                }
            }
        } catch {
            // Out of memory, revert everything we've done.

            // Firstly look for any moved linked entries and free them
            for (int bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++) {
                ref Entry bucket = ref _buckets[bucketIndex];

                if (bucket.Status == EntryStatus.Free)
                    continue;

                int entryIndex = bucket.NextIndex;
                while (entryIndex != -1) {
                    ref Entry entry = ref GetLinkedEntry(entryIndex);

                    if (entry.Status == EntryStatus.Moved) {
                        int nextEntryIndex = entry.NextIndex;
                        FreeLinkedEntry(entryIndex, ref entry);
                        entryIndex = nextEntryIndex;
                    } else {
                        ValidateEntry(ref entry, EntryStatus.Allocated);
                        entryIndex = entry.NextIndex;
                    }
                }
            }

            // Now revert all the stuff we updated
            _buckets = oldBuckets;
            _bucketsLength = oldBucketsLength;
            _bucketsCapacityMultiplier = oldCapacityMultiplier;
        }

        // Now, copy over the linked entries. This step should never throw.
        for (int linkedEntryIndex = 0; linkedEntryIndex < _linkedEntryCount; linkedEntryIndex++) {
            ref Entry linkedEntry = ref GetLinkedEntry(linkedEntryIndex);
            switch (linkedEntry.Status) {
                case EntryStatus.Allocated:
                    ref Entry bucket = ref GetBucket(linkedEntry.HashCode);
                    if (bucket.Status == EntryStatus.Allocated) {
                        // If the bucket is allocated, we can just add ourselfs to the linked list.
                        linkedEntry.NextIndex = bucket.NextIndex;
                        bucket.NextIndex = linkedEntryIndex;
                    } else {
                        // Otherwise, we need to promote this linked entry to a bucket.
                        // Copy it in
                        bucket = linkedEntry;
                        bucket.NextIndex = -1;
                        // Free the old linked entry
                        FreeLinkedEntry(linkedEntryIndex, ref linkedEntry);
                    }
                    break;
                case EntryStatus.Moved:
                    linkedEntry.Status = EntryStatus.Allocated;
                    break;
                case EntryStatus.Free:
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Set(TValueRef valueRef, bool replace) => InlineSet(valueRef, replace);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Set(TValueRef valueRef) => InlineSet(valueRef);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Add(TValueRef valueRef) => InlineAdd(valueRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineAdd(TValueRef valueRef) => InlineSet(valueRef, false);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineSet(TValueRef valueRef, bool replace = true) {
        ValidateSelf();
        IncrementModificationNumber();
        TryResizeBuckets();

        int hashCode = TMethods.GetValueRefHashCode(valueRef);
        ref Entry bucketEntry = ref GetBucket(hashCode);

        int entryIndex = -1;
        ref Entry entry = ref bucketEntry;

        if (bucketEntry.Status == EntryStatus.Allocated) {
            for (; ; ) {
                ValidateEntry(ref entry, EntryStatus.Allocated);
                if (entry.HashCode == hashCode && TMethods.IsValueRefValueEqual(valueRef, in entry.Value)) {
                    if (!replace)
                        return false;
                    TMethods.DereferenceValue(valueRef, out entry.Value);
                    return true;
                }

                if (entry.NextIndex == -1)
                    break;

                entry = ref GetLinkedEntry(entry.NextIndex);
            }

            entry = ref AllocateLinkedEntry(out entryIndex);

            entry.NextIndex = bucketEntry.NextIndex;
            bucketEntry.NextIndex = entryIndex;
        } else {
            entry.NextIndex = -1;
        }

        ValidateEntry(ref entry, EntryStatus.Free);

        entry.Status = EntryStatus.Allocated;
        entry.HashCode = hashCode;
        ++_count;

        TMethods.DereferenceValue(valueRef, out entry.Value);

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [UnscopedRef]
    public ref TValue Get(TKey key) => ref InlineGet(key);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [UnscopedRef]
    public ref TValue InlineGet(TKey key) {
        ValidateSelf();
        IncrementModificationNumber();

        int hashCode = TMethods.GetKeyHashCode(key);
        ref Entry entry = ref GetBucket(hashCode);

        if (entry.Status == EntryStatus.Free)
            return ref Unsafe.NullRef<TValue>();

        for (; ; ) {
            ValidateEntry(ref entry, EntryStatus.Allocated);

            if (entry.HashCode == hashCode && TMethods.IsKeyValueEqual(key, in entry.Value))
                return ref entry.Value;

            if (entry.NextIndex == -1)
                return ref Unsafe.NullRef<TValue>();

            entry = ref GetLinkedEntry(entry.NextIndex);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef replaceValueRef, out Ref<TValue> value)
        => InlineGetOrSet(replaceValueRef, out value);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef replaceValueRef, out Ref<TValue> value) {
        ValidateSelf();
        IncrementModificationNumber();
        TryResizeBuckets();

        int hashCode = TMethods.GetValueRefHashCode(replaceValueRef);

        ref Entry bucketEntry = ref GetBucket(hashCode);
        ref Entry entry = ref bucketEntry;
        int entryIndex = -1;

        if (entry.Status == EntryStatus.Allocated) {
            for (; ; ) {
                ValidateEntry(ref entry, EntryStatus.Allocated);

                if (entry.HashCode == hashCode && TMethods.IsValueRefValueEqual(replaceValueRef, in entry.Value)) {
                    value = new(ref entry.Value);
                    return false;
                }

                if (entry.NextIndex == -1)
                    break;

                entry = ref GetLinkedEntry(entry.NextIndex);
            }

            entry = ref AllocateLinkedEntry(out entryIndex);

            entry.NextIndex = bucketEntry.NextIndex;
            bucketEntry.NextIndex = entryIndex;
        } else {
            entry.NextIndex = -1;
        }


        ValidateEntry(ref entry, EntryStatus.Free);

        entry.Status = EntryStatus.Allocated;
        entry.HashCode = hashCode;
        ++_count;

        TMethods.DereferenceValue(replaceValueRef, out entry.Value);

        value = new(ref entry.Value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) => Remove(key, ref Unsafe.NullRef<TValue>());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, ref TValue outValue) => InlineRemove(key, ref outValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineRemove(TKey key) => InlineRemove(key, ref Unsafe.NullRef<TValue>());

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, ref TValue outValue) {
        ValidateSelf();
        IncrementModificationNumber();

        int hashCode = TMethods.GetKeyHashCode(key);
        ref Entry entry = ref GetBucket(hashCode);
        int entryIndex = -1;
        ref Entry beforeEntry = ref Unsafe.NullRef<Entry>();

        if (entry.Status == EntryStatus.Free)
            return false;

        for (; ; ) {
            ValidateEntry(ref entry, EntryStatus.Allocated);

            if (entry.HashCode == hashCode && TMethods.IsKeyValueEqual(key, in entry.Value)) {
                --_count;

                if (!Unsafe.IsNullRef(ref outValue))
                    outValue = entry.Value;

                // Is this a bucket entry?
                if (entryIndex == -1) {
                    Debug.Assert(Unsafe.IsNullRef(ref beforeEntry));

                    // If so, do we have a link to another entry?
                    if (entry.NextIndex == -1) {
                        // If we have no linked entry, this bucket now has no elements.
                        FreeBucket(ref entry);
                    } else {
                        // If we have a linked entry, we need to promote it to a bucket entry. 
                        // Copy it into the right place
                        int linkedEntryIndex = entry.NextIndex;
                        ref Entry linkedEntry = ref GetLinkedEntry(linkedEntryIndex);
                        entry = linkedEntry;
                        // Free the old linked entry
                        FreeLinkedEntry(linkedEntryIndex, ref linkedEntry);
                    }
                } else {
                    Debug.Assert(!Unsafe.IsNullRef(ref beforeEntry));
                    // This is a linked entry.
                    // Remove it from the linked list and free it
                    beforeEntry.NextIndex = entry.NextIndex;
                    FreeLinkedEntry(entryIndex, ref entry);
                }

                return true;
            }

            beforeEntry = ref entry;
            entryIndex = entry.NextIndex;

            if (entryIndex == -1)
                return false;

            entry = ref GetLinkedEntry(entryIndex);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Clear() => InlineClear();

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void InlineClear() {
        ValidateSelf();
        IncrementModificationNumber();

        _count = 0;
        _linkedEntryCount = 0;
        _linkedFreeListHeadIndex = -1;
        Array.Clear(_buckets);
        Array.Clear(_linkedEntries);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ContainsKey(TKey key) => InlineContainsKey(key);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineContainsKey(TKey key) {
        return InlineGet(key) != null;
    }

    public readonly ref struct RefEnumerable {
        private readonly ref InlineHashTable<TKey, TValue, TValueRef, TMethods> _table;

        public RefEnumerable(ref InlineHashTable<TKey, TValue, TValueRef, TMethods> tbl) {
            _table = ref tbl;
        }

        public RefEnumerator GetEnumerator() => new(ref _table);
    }

    public ref struct RefEnumerator {
        private int _bucket;
        private ref Entry _entry;
        private readonly ref InlineHashTable<TKey, TValue, TValueRef, TMethods> _table;

#if EMBER_SAFETY_CHECKS
        private readonly ulong _modificationNumber;
#endif

        public ref TValue Current {
            get {
                ValidateEntry(ref _entry, EntryStatus.Allocated);
                return ref _entry.Value;
            }
        }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public RefEnumerator(ref InlineHashTable<TKey, TValue, TValueRef, TMethods> tbl) {
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

            if (_bucket == _table._buckets.Length) return false;
            if (_bucket == -1 || _entry.NextIndex == -1) {
                while (++_bucket < _table._buckets.Length && (_entry = ref _table._buckets[_bucket]).Status == EntryStatus.Free) ;
                return _bucket != _table._buckets.Length;
            } else {
                _entry = ref _table.GetLinkedEntry(_entry.NextIndex);
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
        if (_linkedEntryCount < 0) throw new InvalidOperationException($"{nameof(_linkedEntryCount)} < 0");
        if (_count < 0) throw new InvalidOperationException($"{nameof(_count)} < 0");
        if (_buckets == null) throw new NullReferenceException(nameof(_buckets));
        if (_linkedEntries == null) throw new NullReferenceException(nameof(_buckets));
#endif
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private static void ValidateEntry(ref Entry entry, EntryStatus status) {
#if EMBER_SAFETY_CHECKS
        if (entry.Status != status)
            throw new ArgumentException($"{nameof(entry)} did not have the expected status of {status} (got {entry.Status}).");
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

        bool[] entryMap = new bool[_linkedEntries.Length];
        HashSet<TValue> foundValues = [];
        HashSet<int> visitedEntries = [];

        if (_buckets.Length != _bucketsLength)
            Error($"Buckets length out of sync.");

        int expectedEntryCount = 0;
        int bucketCount = 0;
        for (int bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++) {
            ref Entry bucketEntry = ref _buckets[bucketIndex];
            ref Entry entry = ref bucketEntry;
            if (entry.Status == EntryStatus.Allocated) {
                ++bucketCount;
                int perBucketEntryCount = 0;
                int entryIndex = -1;
                for (; ; ) {
                    if (entryIndex != -1 && !visitedEntries.Add(entryIndex)) {
                        Error($"Entry {entryIndex} visited twice.");
                        break;
                    }
                    ++perBucketEntryCount;
                    ++expectedEntryCount;

                    if (entryIndex != -1) {
                        if (entryIndex < 0 || entryIndex >= _linkedEntries.Length) {
                            Error($"Hit invalid entry index in bucket {bucketIndex} at {entryIndex}");
                        } else if (entryMap[entryIndex]) {
                            Error($"Duplicate entry linkage in bucket {bucketIndex} at {entryIndex}");
                        } else if (foundValues.Contains(entry.Value)) {
                            Error($"Duplicate value {entry.Value} at {entryIndex}");
                        } else if (entry.Status == EntryStatus.Free) {
                            Error($"Unallocated entry in bucket {bucketIndex} at {entryIndex}");
                        } else {
                            entryMap[entryIndex] = true;
                        }
                    }

                    if (TMethods.GetValueHashCode(in entry.Value) != entry.HashCode) {
                        Error($"Wrong hash code for value {entry.Value}");
                    } else if (GetBucketIndex(entry.HashCode) != bucketIndex) {
                        Error($"Wrong bucket {bucketIndex} for value {entry.Value} (expected {GetBucketIndex(entry.HashCode)})");
                    }

                    foundValues.Add(entry.Value);

                    entryIndex = entry.NextIndex;

                    if (entryIndex == -1)
                        break;

                    entry = ref GetLinkedEntry(entryIndex);
                }
            }
        }
        if (!error && expectedEntryCount != _count) {
            Error($"Total entry count was {_count} but found {expectedEntryCount}.");
        }

        int freeListLength = 0;
        {
            int entryIndex = _linkedFreeListHeadIndex;
            while (entryIndex != -1) {
                ++freeListLength;
                if (entryIndex < 0 || entryIndex >= _linkedEntries.Length) {
                    Error($"Hit invalid entry index in free list {entryIndex}");
                } else if (entryMap[entryIndex]) {
                    Error($"Accesiable entry in free list {entryIndex}");
                }

                ref Entry entry = ref GetLinkedEntry(entryIndex);

                if (entry.Status == EntryStatus.Allocated) {
                    Error($"Allocated entry in free list {entryIndex}");
                }
                entryIndex = entry.NextIndex;
            }
        }

        int expectedLinkedEntryCount = expectedEntryCount + freeListLength - bucketCount;
        if (!error && expectedLinkedEntryCount != _linkedEntryCount) {
            Error($"Linked entry count was {_linkedEntryCount} but found {expectedLinkedEntryCount}.");
        }

        for (int i = 0; i < _linkedEntries.Length; i++) {
            if (i < expectedLinkedEntryCount) {
                if ((GetLinkedEntry(i).Status == EntryStatus.Allocated) != entryMap[i]) {
                    Error($"Invalid tracked allocation state. {i} is {GetLinkedEntry(i).Status}.");
                }
            } else if (GetLinkedEntry(i).Status == EntryStatus.Allocated) {
                Error($"Entry allocated past linked count!");
            }
        }
        if (error)
            throw new Exception("UnmanagedHashTable validation error/s.");
#endif
    }
}