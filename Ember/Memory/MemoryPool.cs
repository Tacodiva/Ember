
using System;
using Ember.Collections;
using Ember.Utils;

namespace Ember.Memory;

public unsafe sealed class MemoryPool : IDisposable {

    public int AllocationLengthBytes { get; }

    public int AllocationCount { get; private set; }
    public int MaxAllocationCount { get; }

    public int AllocatedSizeBytes => AllocationCount * AllocationLengthBytes;

    public int BorrowedAllocationCount {
        get {
            using (var @lock = ReferenceSpinLock.Aquire(ref _lock)) {
                return AllocationCount - _pool.Count;
            }
        }
    }

    public int BorrowedSizeBytes => BorrowedAllocationCount * AllocationLengthBytes;

    private SpinLockValue _lock;
    private UnmanagedInlineList<Ptr> _pool;

    public MemoryPool(int spanLength, int maxSizeBytes = -1) {
        _pool.Init();
        AllocationLengthBytes = spanLength;
        MaxAllocationCount = maxSizeBytes == -1 ? -1 : maxSizeBytes / AllocationLengthBytes;
    }

    public MemorySpan Borrow() {
        using (var @lock = ReferenceSpinLock.Aquire(ref _lock)) {
            if (_pool.TryPop(out Ptr allocation)) {
                return new(allocation, AllocationLengthBytes);
            }
            ++AllocationCount;
        }

        return MemoryUtils.AllocateMemorySpanUninitialized(AllocationLengthBytes);
    }

    public void Return(MemorySpan span) {
        if (span.LengthLong != AllocationLengthBytes) throw new ArgumentException($"Span of length {span.LengthLong} is not the expected length {AllocationLengthBytes}.");

        ReferenceSpinLock.Aquire(ref _lock);

        if (MaxAllocationCount != -1 && AllocationCount > MaxAllocationCount) {
            --AllocationCount;

            ReferenceSpinLock.Release(ref _lock);

            MemoryUtils.Free(span);
            return;
        }

        _pool.Add(span.Pointer);

        ReferenceSpinLock.Release(ref _lock);
    }

    public void Clear() {
        using (var @lock = ReferenceSpinLock.Aquire(ref _lock)) {
            AllocationCount -= _pool.Count;

            foreach (Ptr allocation in _pool)
                allocation.Free();

            _pool.Clear();
        }
    }

    public void Dispose() {
        Clear();
        _pool.Dispose();
    }
}