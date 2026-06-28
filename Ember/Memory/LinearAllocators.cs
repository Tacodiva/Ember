
using System;
using System.Runtime.CompilerServices;
using Ember.Collections;

namespace Ember.Memory;

public unsafe interface ILinearAllocator : IDisposable {

    /// <summary>
    /// The amount of memory allocated from this allocator in bytes.
    /// </summary>
    public int Size { get; }
    public MemorySpan Allocate(int length);
    public MemorySpan<T> Allocate<T>(int length) where T : unmanaged;
    public T* Allocate<T>() where T : unmanaged;
    public void FreeAll();
}

public unsafe sealed class FixedLinearAllocator : ILinearAllocator {
    public readonly MemorySpan Allocation;
    public int Size { get; private set; }

    public FixedLinearAllocator(int capacity) {
        Allocation = MemoryUtils.AllocateMemorySpanUninitialized(capacity);
        Size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan Allocate(int length) {
        MemorySpan span = Allocation.Slice(Size, length);
        Size += length;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> Allocate<T>(int length) where T : unmanaged {
        return Allocate(length * sizeof(T)).AsMemorySpan<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Allocate<T>() where T : unmanaged {
        return (T*)Allocate(sizeof(T)).Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FreeAll() {
        Size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        MemoryUtils.Free(Allocation);
    }
}

public unsafe sealed class DynamicLinearAllocator : ILinearAllocator {
    public readonly int BlockSize;

    private UnmanagedInlineList<Ptr> _blockAllocations;
    private MemorySpan _currentBlock;

    public int Size { get; private set; }

    public DynamicLinearAllocator(int blockSize) {
        BlockSize = blockSize;
        _blockAllocations.Init();
        _currentBlock = default;
        Size = 0;
    }

    public MemorySpan Allocate(int length) {
        Size += length;
        if (_currentBlock.LengthLong < length) {
            if (length > BlockSize) {
                MemorySpan bigBlock = MemoryUtils.AllocateMemorySpanUninitialized(length);
                _blockAllocations.Add(bigBlock.Pointer);
                return bigBlock;
            }
            _currentBlock = MemoryUtils.AllocateMemorySpanUninitialized(BlockSize);
            _blockAllocations.Add(_currentBlock.Pointer);
        }
        (MemorySpan span, _currentBlock) = _currentBlock.Bisect(length);
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> Allocate<T>(int length) where T : unmanaged {
        return Allocate(length * sizeof(T)).AsMemorySpan<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Allocate<T>() where T : unmanaged {
        return (T*)Allocate(sizeof(T)).Pointer;
    }

    public void FreeAll() {
        foreach (Ptr allocation in _blockAllocations) allocation.Free();
        _blockAllocations.Clear();
        _currentBlock = default;
        Size = 0;
    }

    public void Dispose() {
        FreeAll();
        _blockAllocations.Dispose();
    }
}

public unsafe sealed class PooledLinearAllocator : ILinearAllocator {
    public readonly MemoryPool Pool;
    public int Size { get; private set; }

    private UnmanagedInlineList<Ptr> _poolAllocations, _bigAllocations;
    private MemorySpan _currentAllocation;
    private int _currentBlockOffset;

    public PooledLinearAllocator(MemoryPool pool) {
        Pool = pool;
        _poolAllocations.Init();
        _bigAllocations.Init();
        _currentAllocation = default;
        _currentBlockOffset = 0;
        Size = 0;
    }

    public MemorySpan Allocate(int length) {
        Size += length;
        if (_currentAllocation.LengthLong - _currentBlockOffset < length) {
            if (length > Pool.AllocationLengthBytes) {
                MemorySpan bigBlock = MemoryUtils.AllocateMemorySpanUninitialized(length);
                _bigAllocations.Add(bigBlock.Pointer);
                return bigBlock;
            }
            _currentAllocation = Pool.Borrow();
            _poolAllocations.Add(_currentAllocation.Pointer);
            _currentBlockOffset = 0;
        }
        MemorySpan span = _currentAllocation.Slice(_currentBlockOffset, length);
        _currentBlockOffset += length;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> Allocate<T>(int length) where T : unmanaged {
        return Allocate(length * sizeof(T)).AsMemorySpan<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Allocate<T>() where T : unmanaged {
        return (T*)Allocate(sizeof(T)).Pointer;
    }

    public void FreeAll() {
        foreach (Ptr block in _bigAllocations) block.Free();
        _bigAllocations.Clear();
        foreach (Ptr block in _poolAllocations) Pool.Return(new MemorySpan(block, Pool.AllocationLengthBytes));
        _poolAllocations.Clear();
        _currentAllocation = default;
        _currentBlockOffset = 0;
        Size = 0;
    }

    public void Dispose() {
        FreeAll();
        _poolAllocations.Dispose();
        _bigAllocations.Dispose();
    }
}