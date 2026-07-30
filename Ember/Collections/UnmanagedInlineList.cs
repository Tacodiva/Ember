
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ember.Logging;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;

public unsafe struct UnmanagedInlineList<T> : IList<T>, IReadOnlyList<T>, IDisposable where T : unmanaged {

#if EMBER_SAFETY_CHECKS
    private MemoryGuard _memoryGuard;
#endif

    private int _capacity;
    private int _count;
    private T* _buffer;

    public readonly int Count => _count;
    public readonly bool IsReadOnly => false;
    public readonly bool IsEmpty => Count == 0;

    public readonly T* BasePointer => Count == 0 ? null : _buffer;

    public UnmanagedInlineList(ReadOnlySpan<T> contents) => Init(contents);

    public UnmanagedInlineList(int capacity = 0) => Init(capacity);

    public void Init(ReadOnlySpan<T> contents) {
        _capacity = contents.Length;
        _count = contents.Length;
        _buffer = MemoryUtils.AllocateUninitialized<T>(contents.Length);
        contents.CopyTo(new Span<T>(_buffer, _count));

        InitMemoryGuard();
    }

    public void Init(int capacity = 0) {
        _capacity = capacity;
        _count = 0;
        if (capacity == 0) _buffer = null;
        else _buffer = MemoryUtils.AllocateUninitialized<T>(capacity);

        InitMemoryGuard();
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private void InitMemoryGuard() {
#if EMBER_SAFETY_CHECKS
        _memoryGuard.Init();
#endif
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (_count > _capacity) throw new InvalidOperationException($"{nameof(_count)} > {nameof(_capacity)}");
        if (_count < 0) throw new InvalidOperationException($"{nameof(_count)} < 0");
        if (_capacity < 0) throw new InvalidOperationException($"{nameof(_capacity)} < 0");
        if (_capacity != 0) MemoryUtils.ValidateAllocation<T>(_buffer, $"{nameof(UnmanagedInlineList<T>)}.{nameof(_buffer)}", _capacity);
        _memoryGuard.Validate();
#endif
    }

    public readonly void Dispose() {
        ValidateSelf();
        if (_buffer != null) MemoryUtils.Free(_buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureAdditionalCapacity(int count) {
        ValidateSelf();
        if (_count + count > _capacity) {
            _capacity = (int)BitOperations.RoundUpToPowerOf2((uint)(_count + count));
            _buffer = MemoryUtils.ResizeUninitialized(_buffer, _capacity);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* AddUninitialized() {
        EnsureAdditionalCapacity(1);
        return &_buffer[_count++];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> AddUninitialized(int count) {
        EnsureAdditionalCapacity(count);
        MemorySpan<T> span = new(&_buffer[_count], count);
        _count += count;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T item) {
        EnsureAdditionalCapacity(1);
        _buffer[_count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => Add(in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddAll(MemorySpan<T> items) {
        EnsureAdditionalCapacity(items.Length);
        items.CopyTo(new(&_buffer[_count], items.Length));
        _count += items.Length;
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    private readonly void ValidateCountAtLeast(int minCount) {
        if (_count < minCount) throw new InvalidOperationException($"List must have at least {minCount} elements.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop() {
        ValidateSelf();
        ValidateCountAtLeast(1);
        return _buffer[--_count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Pop(int amount) {
        ValidateSelf();
        ValidateCountAtLeast(amount);
        _count -= amount;
    }

    public bool TryPop(T* outValue) {
        ValidateSelf();
        if (_count == 0) return false;
        *outValue = _buffer[--_count];
        return true;
    }

    public bool TryPop(out T value) {
        ValidateSelf();
        if (_count == 0) {
            value = default;
            return false;
        }

        value = _buffer[--_count];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, in T item) {
        EnsureAdditionalCapacity(1);
        MemoryUtils.Copy(&_buffer[index], &_buffer[index + 1], _count++ - index);
        _buffer[index] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item) => Insert(index, in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T* GetPointer(int index) {
        ValidateSelf();
#if EMBER_SAFETY_CHECKS
        if (index < 0 || index >= _count) throw new IndexOutOfRangeException(nameof(index));
#endif
        return &_buffer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Get(int index) => *GetPointer(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T GetRef(int index) => ref Unsafe.AsRef<T>(GetPointer(index));

    public readonly T this[int index] {
        get => Get(index);
        set => *GetPointer(index) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        ValidateSelf();
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int IndexOf(T item) {
        ValidateSelf();
        T* items = _buffer;
        for (int i = 0; i < _count; i++)
            if (EqualityComparer<T>.Default.Equals(items[i], item))
                return i;
        return -1;
    }

    public readonly void CopyTo(T[] array, int arrayIndex) {
        ValidateSelf();
#if EMBER_SAFETY_CHECKS
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex + _count > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
#endif
        fixed (T* pArray = array)
            MemoryUtils.Copy(_buffer, &pArray[arrayIndex], _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index) {
        ValidateSelf();
#if EMBER_SAFETY_CHECKS
        if (index < 0 || index >= _count) throw new IndexOutOfRangeException(nameof(index));
#endif
        --_count;
        MemoryUtils.Copy(&_buffer[index + 1], &_buffer[index], _count - index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item) {
        int index = IndexOf(item);
        if (index == -1) return false;
        RemoveAt(index);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(T item) {
        return IndexOf(item) != -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ContainsPointer(T* item) {
        ValidateSelf();
        return item >= _buffer && (item - _buffer) < _count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly MemorySpan<T> GetMemorySpan() {
        return new MemorySpan<T>(_buffer, Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int BinarySearchInternal<TComparer>(T searchValue, TComparer comparer) where TComparer : IComparer<T> {
        int lo = 0;
        int high = _count - 1;
        while (lo <= high) {
            int mid = lo + (high - lo) / 2;
            T midElement = _buffer[mid];
            
            int comparison = comparer.Compare(midElement, searchValue);

            if (comparison > 0) high = mid - 1;
            else if (comparison == 0) return mid;
            else lo = mid + 1;
        }
        return ~lo;
    }

    public readonly int BinarySearch(T searchValue)
        => BinarySearchInternal(searchValue, Comparer<T>.Default);

    public readonly int BinarySearch<TComparer>(T searchValue, TComparer comparer) where TComparer : IComparer<T>
        => BinarySearchInternal(searchValue, comparer);

    public readonly int BinarySearch<TComparer>(T searchValue, IComparer<T> comparer)
        => BinarySearchInternal(searchValue, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Sort() => GetMemorySpan().Sort();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<T> => GetMemorySpan().Sort(comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Sort(IComparer<T> comparer) => GetMemorySpan().Sort(comparer);


    public readonly Enumerator GetEnumerator() => new(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        public readonly UnmanagedInlineList<T> List;
        public int Index;

        public T Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
#if EMBER_SAFETY_CHECKS
                if (Index < 0 || Index >= List.Count)
                    throw new IndexOutOfRangeException(nameof(Index));
#endif

                return List._buffer[Index];
            }
        }
        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator(in UnmanagedInlineList<T> list) {
            list.ValidateSelf();
            List = list;
            Index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            return ++Index < List.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() {
            Index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }
    }

}