
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Collections;

public unsafe readonly struct UnmanagedFreeableList<T> : IEnumerable<T>, IDisposable where T : unmanaged {

    public static UnmanagedFreeableList<T> Allocate(int capacity = 16) {
        return new(capacity, UnmanagedIndexAllocator.Allocate());
    }

    private struct Data {
        public int Count;
        public UnmanagedInlineList<T> List;
        public UnmanagedIndexAllocator IndexAllocator;
    }

    private readonly Data* _ptr;
    public bool IsNull => _ptr == null;

    public UnmanagedIndexAllocator.Enumerator AllocatedIndices {
        get {
            ValidateSelf();
            return _ptr->IndexAllocator.GetEnumerator();
        }
    }

    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return _ptr->Count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set {
            _ptr->Count = value;
        }
    }

    private UnmanagedFreeableList(int capacity, UnmanagedIndexAllocator indexAllocator) {
        _ptr = MemoryUtils.AllocateUninitialized<Data>();
        *_ptr = new() {
            Count = 0,
            IndexAllocator = indexAllocator
        };

        _ptr->List.Init(capacity);
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal void ValidateSelf() {
        if (IsNull) throw new NullReferenceException($"{nameof(UnmanagedFreeableList<T>)} is null.");

        _ptr->List.ValidateSelf();
        _ptr->IndexAllocator.ValidateSelf();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index) {
        ValidateSelf();
        return _ptr->List[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int index) {
        return ref _ptr->List.GetRef(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(int index) {
        return _ptr->List.GetPointer(index);
    }

    public int Add(in T item) {
        *AddUninitialized(out int index) = item;
        return index;
    }

    public T* AddUninitialized(out int index, bool initNew = false) {
        ValidateSelf();
        index = _ptr->IndexAllocator.AllocateIndex();
        ++Count;

        Debug.Assert(index <= _ptr->List.Count);

        if (index == _ptr->List.Count) {
            T* ptr = _ptr->List.AddUninitialized();
            if (initNew) *ptr = default;
            return ptr;
        } else {
            return &_ptr->List.BasePointer[index];
        }
    }

    public void Free(int index) {
        ValidateSelf();
        _ptr->IndexAllocator.FreeIndex(index);
        --Count;
    }

    public bool IsIndexAllocated(int index) {
        ValidateSelf();
        return _ptr->IndexAllocator.IsIndexAllocated(index);
    }

    public void Clear() {
        ValidateSelf();

        _ptr->IndexAllocator.Clear();
        _ptr->List.Clear();
        Count = 0;
    }

    public ref T this[int index] {
        get => ref GetRef(index);
    }

    public void Dispose() {
        ValidateSelf();
        _ptr->List.Dispose();
        _ptr->IndexAllocator.Dispose();
        MemoryUtils.Free(_ptr);
    }


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(this);

    public struct Enumerator : IEnumerator<T> {
        public readonly UnmanagedIndexAllocator IndexAllocator;
        public UnmanagedInlineList<T>.Enumerator ListEnumerator;

        public T Current => ListEnumerator.Current;
        object IEnumerator.Current => Current;

        public Enumerator(UnmanagedFreeableList<T> list) {
            list.ValidateSelf();
            IndexAllocator = list._ptr->IndexAllocator;
            ListEnumerator = list._ptr->List.GetEnumerator();
        }

        public bool MoveNext() {
            bool hasNext;
            do {
                hasNext = ListEnumerator.MoveNext();
            } while (hasNext && !IndexAllocator.IsIndexAllocated(ListEnumerator.Index));

            return hasNext;
        }

        public void Reset() {
            ListEnumerator.Reset();
        }

        public void Dispose() {
            ListEnumerator.Dispose();
        }
    }

}