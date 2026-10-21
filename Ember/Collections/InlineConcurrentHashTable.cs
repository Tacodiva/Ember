
#if !DEBUG
#define EMBER_INLINE
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ember.Utils;

namespace Ember.Collections;

public unsafe interface IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut>
    : IHashTableMethods<TKey, TValue, TValueRef>
    where TKey : allows ref struct
    where TValueRef : allows ref struct
    where TValueOut : allows ref struct {

    public static abstract bool HasAtomicValueOverwrite {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // When called, it is guarenteed that IsValueRefValueEqual(valueRef, value) and HasAtomicValueOverwrite are true.
    public static abstract void AtomicValueOverwrite(TValueRef valueRef, ref TValue value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void CopyValueOut(in TValue value, out TValueOut valueOut);

}

public static class InlineConcurrentHashTable {

    /** Note that enumeration concurrent with modification is not supported, and may result in incomplete data being returned */
    public static InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>.RefEnumerable Values<TKey, TValue, TValueRef, TValueOut, TMethods>(this ref InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> @this)
        where TKey : allows ref struct
        where TValueRef : allows ref struct
        where TValueOut : allows ref struct
        where TMethods : struct, IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut> {
        return new(ref @this);
    }
}


public unsafe struct InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods>
    where TKey : allows ref struct
    where TValueRef : allows ref struct
    where TValueOut : allows ref struct
    where TMethods : struct, IConcurrentHashTableMethods<TKey, TValue, TValueRef, TValueOut> {

    private class Node {

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Node Create(int hashCode, in TValue value) => new(hashCode, value);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Node Create(int hashCode, TValueRef valueRef) => new(hashCode, valueRef);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Node Create<TArg>(int hashCode, ValueFactory<TArg> valueFactory, in TArg arg) where TArg : allows ref struct {
            Node n = new(hashCode, default(TValue)!);
            valueFactory(arg, out n.Value);
            return n;
        }

        public readonly int HashCode;
        public bool IsCollision;
        public Node? Next;
        public TValue Value;

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private Node(int hashCode, in TValue value) {
            HashCode = hashCode;
            Value = value;
        }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private Node(int hashCode, TValueRef valueRef) {
            HashCode = hashCode;
            TMethods.DereferenceValue(valueRef, out Value);
        }
    }

    // This will pad out Bucket to be at least 32 bytes, so with 128-bit cache lines that's 4 buckets on the same
    //   line. We want to avoid false sharing but not use too much memory, I hope that's an okay compromise.
    private const int MinBucketSize = 32;
    [StructLayout(LayoutKind.Sequential, Size = MinBucketSize)]
    private struct Bucket {
        /* While in a bucket's lock, no node in the bucket may be modified. */
        public SlimSpinLock Lock;
        public bool IsResizeBlocked;
        public ulong ModificationNumber;
        public int Count;
        public int CountWithoutCollisions;
        public Node? Head;

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AcquireLock() {
            SlimSpinLock.Acquire(ref Lock);
        }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ReleaseLock() {
            SlimSpinLock.Release(ref Lock);
        }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void IncrementModificationNumber() {
            Volatile.Write(ref ModificationNumber, ModificationNumber + 1);
        }

        public readonly bool IsResizeRequested {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Volatile.Read(in CountWithoutCollisions) >= CheckResizeCount && !IsResizeBlocked;
        }

    }

    private sealed class BucketTable(int capacity) {
        public readonly Bucket[] Buckets = new Bucket[capacity];
        public readonly ulong CapacityMultiplier = HashUtils.GetFastModMultiplier((uint)capacity);
        public readonly int Capacity = capacity;

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ref Bucket GetBucketRefFromHash(int hash)
            => ref GetBucketRefFromIndex(GetBucketIndexFromHash(hash));


#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ref Bucket GetBucketRefFromIndex(int index) {
            Debug.Assert(index >= 0 && index < Buckets.Length);
            return ref Buckets.UnsafeGetReference(index);
        }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetBucketIndexFromRef(ref Bucket bucket)
            => Buckets.GetReferenceIndex(ref bucket);


#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetBucketIndexFromHash(int hash) {
            return (int)HashUtils.FastMod((uint)hash, (uint)Capacity, CapacityMultiplier);
        }

    }

    private const int CheckResizeCount = 2;

    private AtomicBool _bucketsResizeLock;
    private BucketTable _table;

#if EMBER_SAFETY_CHECKS
    private uint _modificationNumber;
#endif

    public readonly int Count {
        get {
            ValidateSelf();

            int count = 0;

            BucketTable table = Volatile.Read(in _table);

            for (int i = 0; i < table.Capacity; i++) {
                ref Bucket bucket = ref table.GetBucketRefFromIndex(i);
                bucket.AcquireLock();
                count += Volatile.Read(in bucket.Count);
            }

            for (int i = 0; i < table.Capacity; i++)
                table.GetBucketRefFromIndex(i).ReleaseLock();

            return count;
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public InlineConcurrentHashTable(int capacity = 16) => Init(capacity);
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void Init(int capacity = 16) {
        try {
            _bucketsResizeLock = false;
            _table = new(HashUtils.GetPrime(capacity));
        } catch {
            // Out of memory
            this = default;
            throw;
        }
    }

    private void ResizeBuckets(BucketTable oldTable, ref Bucket triggerBucket) {
        if (!_bucketsResizeLock.FalseToTrue()) {
            SpinWait sw = new();
            do {
                sw.SpinOnce();
                if (!ReferenceEquals(Volatile.Read(ref _table), oldTable))
                    return;
            } while (!_bucketsResizeLock.FalseToTrue());
        }

        try {
            if (!ReferenceEquals(Volatile.Read(ref _table), oldTable))
                return;

            int newCapacity = HashUtils.ExpandPrime(oldTable.Capacity);

            // Can throw OOM, so don't modify anything until this has returned
            BucketTable newTable = new(newCapacity);
            int bucketLockIndex = -1;

            try {
                // Lock all the buckets and count up the entries
                for (int oldBucketIndex = 0; oldBucketIndex < oldTable.Capacity; oldBucketIndex++) {
                    ref Bucket oldBucket = ref oldTable.GetBucketRefFromIndex(oldBucketIndex);
                    oldBucket.AcquireLock();
                    bucketLockIndex = oldBucketIndex;

                    Node? oldNode = oldBucket.Head;
                    while (oldNode != null) {
                        ref Bucket newBucket = ref newTable.GetBucketRefFromHash(oldNode.HashCode);

                        Node newNode = Node.Create(oldNode.HashCode, in oldNode.Value);

                        {
                            Node? bucketNode = newBucket.Head;
                            while (bucketNode != null) {
                                if (bucketNode.HashCode == oldNode.HashCode) {
                                    newNode.IsCollision = true;
                                    break;
                                }
                                bucketNode = bucketNode.Next;
                            }
                        }

                        newNode.Next = newBucket.Head;
                        newBucket.Head = newNode;

                        ++newBucket.Count;

                        if (!newNode.IsCollision) {
                            ++newBucket.CountWithoutCollisions;

                            if (newBucket.CountWithoutCollisions >= CheckResizeCount)
                                newBucket.IsResizeBlocked = true;
                        }

                        oldNode = oldNode.Next;
                    }
                }

                // We've resized the array yay! Now swap it out
                Volatile.Write(ref _table, newTable);
                IncrementModificationNumber();
            } finally {
                // Release all the old locks
                for (int i = 0; i <= bucketLockIndex; i++)
                    oldTable.GetBucketRefFromIndex(i).ReleaseLock();
            }
        } finally {
            _bucketsResizeLock = false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Set(TValueRef valueRef) => InlineSet(valueRef);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineSet(TValueRef valueRef) {
        ValidateSelf();

        int hashCode = TMethods.GetValueRefHashCode(valueRef);

        Node? newNode = null;

        if (!TMethods.HasAtomicValueOverwrite) {
            // If we don't have an atomic value overwrite we know we'll need a new node,
            //   best to do it here outside the lock
            newNode = Node.Create(hashCode, valueRef);
        }

        ref Bucket bucket = ref Unsafe.NullRef<Bucket>();

        for (; ; ) {
            BucketTable table = Volatile.Read(ref _table);
            bucket = ref table.GetBucketRefFromHash(hashCode);

            bucket.AcquireLock();

            if (bucket.IsResizeRequested) {
                bucket.ReleaseLock();
                ResizeBuckets(table, ref bucket);
                continue;
            }

            if (ReferenceEquals(table, Volatile.Read(ref _table)))
                break;

            bucket.ReleaseLock();
        }

        IncrementModificationNumber();

        Node? oldNode = Volatile.Read(ref bucket.Head);
        Node? oldNodePrevious = null;
        bool isCollision = false;

        try {
            while (oldNode != null) {
                if (oldNode.HashCode == hashCode) {
                    if (TMethods.IsValueRefValueEqual(valueRef, in oldNode.Value)) {
                        break;
                    } else {
                        isCollision = true;

                        if (!Volatile.Read(in oldNode.IsCollision)) {
                            Volatile.Write(ref oldNode.IsCollision, true);
                            Volatile.Write(ref bucket.CountWithoutCollisions,
                                Volatile.Read(in bucket.CountWithoutCollisions) - 1
                            );
                        }
                    }
                }

                oldNodePrevious = oldNode;
                oldNode = Volatile.Read(ref oldNode.Next);
            }

            if (oldNode != null) {
                // We're replacing something old

                if (TMethods.HasAtomicValueOverwrite) {
                    TMethods.AtomicValueOverwrite(valueRef, ref oldNode.Value);
                } else {
                    Debug.Assert(newNode != null);
                    Volatile.Write(ref newNode.Next, oldNode.Next);

                    if (oldNodePrevious == null) {
                        Debug.Assert(bucket.Head == oldNode);
                        Volatile.Write(ref bucket.Head, newNode);
                    } else {
                        Debug.Assert(oldNodePrevious.Next == oldNode);
                        Volatile.Write(ref oldNodePrevious.Next, newNode);
                    }

                    bucket.IncrementModificationNumber();
                }

                return true;
            }

            if (TMethods.HasAtomicValueOverwrite)
                newNode = Node.Create(hashCode, valueRef);

            // We're adding something new
            Debug.Assert(newNode != null);
            Volatile.Write(ref newNode.Next, bucket.Head);
            Volatile.Write(ref newNode.IsCollision, isCollision);
            Volatile.Write(ref bucket.Head, newNode);

            Volatile.Write(ref bucket.Count, Volatile.Read(ref bucket.Count) + 1);
            if (!isCollision) {
                Volatile.Write(ref bucket.CountWithoutCollisions,
                    Volatile.Read(in bucket.CountWithoutCollisions) + 1
                );
            }

            bucket.IncrementModificationNumber();
        } finally {
            bucket.ReleaseLock();
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(TKey key) => InlineTryGet(key, ref Unsafe.NullRef<TValueOut?>());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGet(TKey key, [NotNullWhen(true)] ref TValueOut? outValue) => InlineTryGet(key, ref outValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineTryGet(TKey key) => InlineTryGet(key, ref Unsafe.NullRef<TValueOut?>());

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineTryGet(TKey key, [NotNullWhen(true)] ref TValueOut? outValue) {
        ValidateSelf();

        int hashCode = TMethods.GetKeyHashCode(key);

        BucketTable table = Volatile.Read(ref _table);
        ref Bucket bucket = ref table.GetBucketRefFromHash(hashCode);
        Node? node = Volatile.Read(ref bucket.Head);

        while (node != null) {
            if (node.HashCode == hashCode && TMethods.IsKeyValueEqual(key, in node.Value)) {
                if (!Unsafe.IsNullRef(ref outValue))
                    TMethods.CopyValueOut(in node.Value, out outValue);
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
            }
            node = Volatile.Read(ref node.Next);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryAdd(TValueRef valueRef) => InlineTryAdd(valueRef);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineTryAdd(TValueRef valueRef) => InlineGetOrSet(valueRef, ref Unsafe.NullRef<TValueOut>());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool GetOrSet(TValueRef valueRef, ref TValueOut outValue) => InlineGetOrSet(valueRef, ref outValue);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet(TValueRef valueRef, ref TValueOut outValue) {
        ValidateSelf();

        int hashCode = TMethods.GetValueRefHashCode(valueRef);

        // We do a fast-fail lock free read, and if we don't find a matching entry then we lock
        BucketTable table = Volatile.Read(ref _table);
        ref Bucket bucket = ref table.GetBucketRefFromHash(hashCode);
        ulong bucketModificationNumber = Volatile.Read(ref bucket.ModificationNumber);
        bool isCollision = false;

        {
            Node? node = Volatile.Read(ref bucket.Head);

            while (node != null) {
                if (node.HashCode == hashCode) {
                    if (TMethods.IsValueRefValueEqual(valueRef, in node.Value)) {
                        if (!Unsafe.IsNullRef(ref outValue))
                            TMethods.CopyValueOut(in node.Value, out outValue);

                        return false;
                    } else {
                        isCollision = true;
                    }
                }
                node = Volatile.Read(ref node.Next);
            }
        }

        // This is possibly expensive, so we do it here outside any locks, and if it turns out we don't
        //   need it, it will just get garbage collected
        Node newNode = Node.Create(hashCode, valueRef);

        // We didn't find anything matching, now let's grab the bucket's lock for real
        bool didModify = false;
        for (; ; ) {
            bucket.AcquireLock();

            if (bucket.IsResizeRequested) {
                bucket.ReleaseLock();
                ResizeBuckets(table, ref bucket);
            } else {
                if (ReferenceEquals(table, Volatile.Read(ref _table))) {
                    didModify |= Volatile.Read(ref bucket.ModificationNumber) != bucketModificationNumber;
                    break;
                }

                bucket.ReleaseLock();
            }

            didModify = true;
            table = Volatile.Read(ref _table);
            bucket = ref table.GetBucketRefFromHash(hashCode);
        }

        try {
            if (didModify || isCollision) {
                // We modified the table in between our lockless read and now, we need to re-check the table
                Node? node = bucket.Head;
                while (node != null) {
                    if (node.HashCode == hashCode) {
                        if (TMethods.IsValueRefValueEqual(valueRef, in node.Value)) {
                            if (!Unsafe.IsNullRef(ref outValue))
                                TMethods.CopyValueOut(in node.Value, out outValue);

                            return false;
                        } else {
                            isCollision = true;

                            if (!Volatile.Read(in node.IsCollision)) {
                                Volatile.Write(ref node.IsCollision, true);
                                Volatile.Write(ref bucket.CountWithoutCollisions,
                                    Volatile.Read(in bucket.CountWithoutCollisions) - 1
                                );
                            }
                        }
                    }
                    node = Volatile.Read(ref node.Next);
                }
            }

            IncrementModificationNumber();

            Volatile.Write(ref newNode.Next, bucket.Head);
            Volatile.Write(ref newNode.IsCollision, isCollision);
            Volatile.Write(ref bucket.Head, newNode);

            Volatile.Write(ref bucket.Count, Volatile.Read(ref bucket.Count) + 1);
            if (!isCollision) {
                Volatile.Write(ref bucket.CountWithoutCollisions,
                    Volatile.Read(in bucket.CountWithoutCollisions) + 1
                );
            }

            bucket.IncrementModificationNumber();

            if (!Unsafe.IsNullRef(ref outValue))
                TMethods.CopyValueOut(in newNode.Value, out outValue);
        } finally {
            bucket.ReleaseLock();
        }

        return true;
    }

    public delegate void ValueFactory<TArg>(in TArg arg, out TValue dst) where TArg : allows ref struct;

    public bool GetOrSet<TArg>(TKey key, ref TValueOut outValue, ValueFactory<TArg> factory, in TArg argument) {
        return InlineGetOrSet<TArg>(key, ref outValue, factory, argument);
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineGetOrSet<TArg>(TKey key, ref TValueOut outValue, ValueFactory<TArg> factory, in TArg argument) where TArg : allows ref struct {
        ValidateSelf();

        int hashCode = TMethods.GetKeyHashCode(key);

        // We do a fast-fail lock free read, and if we don't find a matching entry then we lock
        BucketTable table = Volatile.Read(ref _table);
        ref Bucket bucket = ref table.GetBucketRefFromHash(hashCode);
        ulong bucketModificationNumber = Volatile.Read(ref bucket.ModificationNumber);
        bool isCollision = false;

        {
            Node? node = Volatile.Read(ref bucket.Head);

            while (node != null) {
                if (node.HashCode == hashCode) {
                    if (TMethods.IsKeyValueEqual(key, in node.Value)) {
                        if (!Unsafe.IsNullRef(ref outValue))
                            TMethods.CopyValueOut(in node.Value, out outValue);

                        return false;
                    } else {
                        isCollision = true;
                    }
                }
                node = Volatile.Read(ref node.Next);
            }
        }

        // This is possibly expensive, so we do it here outside any locks, and if it turns out we don't
        //   need it, it will just get garbage collected
        Node newNode = Node.Create(hashCode, factory, argument);

        // We didn't find anything matching, now let's grab the bucket's lock for real
        bool didModify = false;
        for (; ; ) {
            bucket.AcquireLock();

            if (bucket.IsResizeRequested) {
                bucket.ReleaseLock();
                ResizeBuckets(table, ref bucket);
            } else {
                if (ReferenceEquals(table, Volatile.Read(ref _table))) {
                    didModify |= Volatile.Read(ref bucket.ModificationNumber) != bucketModificationNumber;
                    break;
                }

                bucket.ReleaseLock();
            }

            didModify = true;
            table = Volatile.Read(ref _table);
            bucket = ref table.GetBucketRefFromHash(hashCode);
        }

        try {
            if (didModify || isCollision) {
                // We modified the table in between our lockless read and now, we need to re-check the table
                Node? node = bucket.Head;
                while (node != null) {
                    if (node.HashCode == hashCode) {
                        if (TMethods.IsKeyValueEqual(key, in node.Value)) {
                            if (!Unsafe.IsNullRef(ref outValue))
                                TMethods.CopyValueOut(in node.Value, out outValue);

                            return false;
                        } else {
                            isCollision = true;

                            if (!Volatile.Read(in node.IsCollision)) {
                                Volatile.Write(ref node.IsCollision, true);
                                Volatile.Write(ref bucket.CountWithoutCollisions,
                                    Volatile.Read(in bucket.CountWithoutCollisions) - 1
                                );
                            }
                        }
                    }
                    node = Volatile.Read(ref node.Next);
                }
            }

            IncrementModificationNumber();

            Volatile.Write(ref newNode.Next, bucket.Head);
            Volatile.Write(ref newNode.IsCollision, isCollision);
            Volatile.Write(ref bucket.Head, newNode);

            Volatile.Write(ref bucket.Count, Volatile.Read(ref bucket.Count) + 1);
            if (!isCollision) {
                Volatile.Write(ref bucket.CountWithoutCollisions,
                    Volatile.Read(in bucket.CountWithoutCollisions) + 1
                );
            }

            bucket.IncrementModificationNumber();

            if (!Unsafe.IsNullRef(ref outValue))
                TMethods.CopyValueOut(in newNode.Value, out outValue);
        } finally {
            bucket.ReleaseLock();
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) => Remove(key, ref Unsafe.NullRef<TValueOut?>());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Remove(TKey key, [NotNullWhen(true)] ref TValueOut? outValue) => InlineRemove(key, ref outValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InlineRemove(TKey key) => InlineRemove(key, ref Unsafe.NullRef<TValueOut?>());

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineRemove(TKey key, [NotNullWhen(true)] ref TValueOut? outValue) {
        ValidateSelf();

        int hashCode = TMethods.GetKeyHashCode(key);

        // We do a fast-fail lock free read, and if we find a matching entry then we lock
        BucketTable table = Volatile.Read(ref _table);
        ref Bucket bucket = ref table.GetBucketRefFromHash(hashCode);
        ulong bucketModificationNumber = Volatile.Read(ref bucket.ModificationNumber);

        Node? node = Volatile.Read(ref bucket.Head);
        Node? nodePrevious = null;

        while (node != null) {
            if (node.HashCode == hashCode && TMethods.IsKeyValueEqual(key, in node.Value))
                break;
            nodePrevious = node;
            node = Volatile.Read(ref node.Next);
        }

        // If we didn't find a matching node then we failed to remove
        if (node == null) return false;

        // We didn't find anything matching, now let's grab the bucket's lock for real
        bool didModify = false;
        for (; ; ) {
            bucket.AcquireLock();

            if (ReferenceEquals(table, Volatile.Read(ref _table))) {
                didModify |= Volatile.Read(ref bucket.ModificationNumber) != bucketModificationNumber;
                break;
            }

            bucket.ReleaseLock();

            didModify = true;
            table = Volatile.Read(ref _table);
            bucket = ref table.GetBucketRefFromHash(hashCode);
        }

        try {
            if (didModify) {
                node = Volatile.Read(ref bucket.Head);
                nodePrevious = null;

                while (node != null) {
                    if (node.HashCode == hashCode && TMethods.IsKeyValueEqual(key, in node.Value))
                        break;
                    nodePrevious = node;
                    node = Volatile.Read(ref node.Next);
                }

                // If we didn't find a matching node then we failed to remove
                if (node == null) return false;
            }

            IncrementModificationNumber();

            if (nodePrevious == null) {
                Debug.Assert(bucket.Head == node);
                Volatile.Write(ref bucket.Head, node.Next);
            } else {
                Debug.Assert(nodePrevious.Next == node);
                Volatile.Write(ref nodePrevious.Next, node.Next);
            }

            Volatile.Write(ref bucket.Count, Volatile.Read(ref bucket.Count) - 1);
            if (!Volatile.Read(in node.IsCollision)) {
                Volatile.Write(ref bucket.CountWithoutCollisions,
                    Volatile.Read(in bucket.CountWithoutCollisions) - 1
                );
            }

            bucket.IncrementModificationNumber();

            if (!Unsafe.IsNullRef(ref outValue))
                TMethods.CopyValueOut(in node.Value, out outValue);
        } finally {
            bucket.ReleaseLock();
        }

#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
        return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Clear() => InlineClear();

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    /* Removes all entries from the hash table. The table may be read or written to in a partially cleared state. */
    public void InlineClear() {
        ValidateSelf();

        // Aquire locks for the bucket resize to make sure _table isn't changed as we're clearing
        SlimSpinLock.SpinUntilCompareExchange(ref _bucketsResizeLock, true, false);
        BucketTable table = Volatile.Read(ref _table);

        for (int i = 0; i < table.Capacity; i++) {
            ref Bucket bucket = ref table.GetBucketRefFromIndex(i);
            bucket.AcquireLock();

            Volatile.Write(ref bucket.Count, 0);
            Volatile.Write(ref bucket.CountWithoutCollisions, 0);
            Volatile.Write(ref bucket.Head, default);
            bucket.IsResizeBlocked = false;
            bucket.IncrementModificationNumber();

            bucket.ReleaseLock();
        }

        IncrementModificationNumber();
        _bucketsResizeLock = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ContainsKey(TKey key) => InlineContainsKey(key);

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public bool InlineContainsKey(TKey key) => InlineTryGet(key);

    [Conditional("EMBER_SAFETY_CHECKS")]
    private readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (_table == null) throw new NullReferenceException(nameof(_table));
#endif
    }

#if EMBER_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Conditional("EMBER_SAFETY_CHECKS")]
    private void IncrementModificationNumber() {
#if EMBER_SAFETY_CHECKS
        Interlocked.Increment(ref _modificationNumber);
#endif
    }

    public readonly ref struct RefEnumerable {
        public readonly ref InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> Table;

        internal RefEnumerable(ref InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> table) {
            Table = ref table;
        }

        public RefEnumerator GetEnumerator() => new(ref Table);
    }

    public ref struct RefEnumerator {
        private Node? _node;
        private int _bucketIndex;
        private TValueOut _current;
        private readonly ref InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> _table;
        private readonly uint _modificationNumber;

        public readonly TValueOut Current => _current;

        internal RefEnumerator(ref InlineConcurrentHashTable<TKey, TValue, TValueRef, TValueOut, TMethods> tbl) {
            _table = ref tbl;
            _table.ValidateSelf();
#if EMBER_SAFETY_CHECKS
            _modificationNumber = Volatile.Read(in _table._modificationNumber);
#endif
            _current = default!;
            Reset();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowCollectionModified()
            => throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("EMBER_SAFETY_CHECKS")]
        private void CheckModificationNumber() {
#if EMBER_SAFETY_CHECKS
            if (_modificationNumber != Volatile.Read(ref _table._modificationNumber))
                ThrowCollectionModified();
#endif
        }

        public bool MoveNext() {
            _table.ValidateSelf();

            if (_node != null)
                _node = Volatile.Read(in _node.Next);

            if (_node == null) {
                BucketTable table = Volatile.Read(ref _table._table);

                do {
                    ++_bucketIndex;

                    if (_bucketIndex >= table.Capacity) {
                        CheckModificationNumber();
                        return false;
                    }

                    _node = Volatile.Read(in table.GetBucketRefFromIndex(_bucketIndex).Head);
                } while (_node == null);
            }

            CheckModificationNumber();

            TMethods.CopyValueOut(in _node.Value, out _current);

            return true;
        }

        public void Reset() {
            _node = null;
            _bucketIndex = -1;
        }
    }
}