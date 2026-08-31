
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ember.Collections;

public struct InlineList<T> : IList<T> {

    private T[]? _items;
    private int _count;

    public void Init(int capacity = 2) {
        if (capacity != 0) {
            _items = new T[capacity];
        }

        _count = 0;
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal readonly void ValidateSelf() {
#if EMBER_SAFETY_CHECKS
        if (_items == null) {
            if (_count != 0) throw new InvalidOperationException($"{nameof(_count)} != 0");
        } else {
            if (_count > _items.Length) throw new InvalidOperationException($"{nameof(_count)} > {nameof(_items)}.Length");
            if (_count < 0) throw new InvalidOperationException($"{nameof(_count)} < 0");
        }
#endif
    }

    public readonly T this[int index] {
        get => Get(index);
        set => Get(index) = value;
    }

    public readonly int Count => _count;
    public readonly bool IsReadOnly => false;
    public readonly ArraySegment<T> Contents => _items == null ? ArraySegment<T>.Empty : new(_items, 0, _count);

    public readonly ref T Get(int index) {
        ValidateSelf();
        if (index >= _count) throw new IndexOutOfRangeException(nameof(index));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        return ref _items![index];
    }

    public readonly void Set(int index, in T value) {
        Get(index) = value;
    }

    private int GetNewCapacity(int minimum) {
        Debug.Assert((_items?.Length ?? 0) <= minimum);
        int newCapacity;

        if (_items == null) newCapacity = 2;
        else newCapacity = _items.Length * 2;

        if (newCapacity <= minimum) newCapacity = minimum + 1;

        return newCapacity;
    }

    public void Add(T item) {
        AddUninitialized() = item;
    }

    public ref T AddUninitialized() {
        ValidateSelf();

        if (_items == null) {
            _items = new T[GetNewCapacity(1)];
            _count = 1;
            return ref _items[0];
        }

        int index = _count++;

        if (_items.Length <= index) {
            Array.Resize(ref _items, GetNewCapacity(index));
        }

        return ref _items[index];
    }

    public void Clear() {
        ValidateSelf();

        if (_items == null)
            return;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            for (int i = 0; i < _count; i++)
                _items[i] = default!;
        }

        _count = 0;
    }

    public readonly bool Contains(T item) {
        ValidateSelf();
        return Contents.Contains(item);
    }

    public readonly void CopyTo(T[] array, int arrayIndex) {
        ValidateSelf();
        Contents.CopyTo(array, arrayIndex);
    }

    public readonly ArraySegment<T>.Enumerator GetEnumerator() {
        ValidateSelf();
        return Contents.GetEnumerator();
    }

    public readonly int IndexOf(T item) {
        ValidateSelf();
        return _items == null ? -1 : Array.IndexOf(_items, item, 0, _count);
    }

    internal void GrowForInsertion(int indexToInsert, int insertionCount = 1) {
        Debug.Assert(insertionCount > 0);

        int requiredCapacity = _count + insertionCount;
        int newCapacity = GetNewCapacity(requiredCapacity);

        T[] newItems = new T[newCapacity];
        if (indexToInsert != 0) {
            Array.Copy(_items!, newItems, length: indexToInsert);
        }

        if (_count != indexToInsert) {
            Array.Copy(_items!, indexToInsert, newItems, indexToInsert + insertionCount, _count - indexToInsert);
        }

        _items = newItems;
    }


    public void Insert(int index, T item) {
        InsertUninitialized(index) = item;
    }

    public ref T InsertUninitialized(int index) {
        ValidateSelf();
        if (index > _count) throw new IndexOutOfRangeException(nameof(index));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        if (_items == null) {
            Debug.Assert(index == 0);
            _items = new T[GetNewCapacity(1)];
        } else if (_count == _items.Length) {
            GrowForInsertion(index, 1);
        } else if (index < _count) {
            Array.Copy(_items, index, _items, index + 1, _count - index);
        }

        _count++;
        return ref _items[index];
    }

    public bool Remove(T item) {
        int index = IndexOf(item);
        if (index >= 0) {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index) {
        if (index >= _count) throw new IndexOutOfRangeException(nameof(index));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        --_count;
        if (index < _count) {
            Array.Copy(_items!, index + 1, _items!, index, _count - index);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            _items![_count] = default!;
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
}