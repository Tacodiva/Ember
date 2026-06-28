
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ember.Memory;
using Ember.Utils;

namespace Ember.Collections;


public unsafe readonly struct UnmanagedRingList<T> : IEnumerable<T>, IDisposable where T : unmanaged {

    public static UnmanagedRingList<T> Null => default;

    public static UnmanagedRingList<T> Allocate(int capacity = 16) {
        return new UnmanagedRingList<T>(capacity);
    }

    private struct Data {
        public int Capacity;
        public int Count;
        public T* Buffer;
        public T* BufferEnd;
        public T* Head;
        public T* Tail;
    }
    private readonly Data* _ptr;

    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _ptr->Count;
    }
    public bool IsReadOnly => false;
    public bool IsEmpty => Count == 0;
    public bool IsNull => _ptr == null;

    private UnmanagedRingList(int capacity) {
        _ptr = MemoryUtils.AllocateUninitialized<Data>();
        _ptr->Capacity = capacity;
        T* items = _ptr->Buffer = MemoryUtils.AllocateUninitialized<T>(capacity);
        _ptr->BufferEnd = &items[capacity];
        _ptr->Head = items;
        _ptr->Tail = items;
        _ptr->Count = 0;
    }

    public void Dispose() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        MemoryUtils.Free(_ptr->Buffer);
        MemoryUtils.Free(_ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int count) {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        if (Count + count > _ptr->Capacity) {
            int oldCapacity = _ptr->Capacity;
            int newCapacity = _ptr->Capacity = oldCapacity * 2;

            if (newCapacity <= Count + count)
                newCapacity = _ptr->Capacity = Count + count;

            T* oldBuffer = _ptr->Buffer;
            T* newBuffer = _ptr->Buffer = MemoryUtils.ResizeUninitialized(_ptr->Buffer, newCapacity);

            T* oldBufferEnd = &newBuffer[oldCapacity];
            T* newBufferEnd = _ptr->BufferEnd = &newBuffer[newCapacity];

            _ptr->Tail = _ptr->Tail + (newBuffer - oldBuffer);
            _ptr->Head = _ptr->Head + (newBuffer - oldBuffer);

            if (_ptr->Head >= _ptr->Tail) {
                T* oldHead = _ptr->Head;
                T* newHead = _ptr->Head = &oldHead[newCapacity - oldCapacity];
                MemoryUtils.Copy((byte*)oldHead, (byte*)newHead, (int)((nint)oldBufferEnd - (nint)oldHead));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* PushBackUninitialized() {
        EnsureCapacity(1);

        T* head = _ptr->Head;
        if (head == _ptr->Buffer) head = &_ptr->BufferEnd[-1];
        else head = &head[-1];
        _ptr->Count += 1;

        return _ptr->Head = head;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBack(in T item) {
        T* ptr = PushBackUninitialized();
        *ptr = item;
        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBack(T item) => PushBack(in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T PopBack() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
        if (Count == 0) throw new InvalidOperationException("List is empty.");
#endif
        T* item = _ptr->Head;
        T* head = &item[1];
        if (head == _ptr->BufferEnd) head = _ptr->Buffer;
        --_ptr->Count;
        _ptr->Head = head;

        return *item;
    }

    public bool TryPopBack(out T value) {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        if (Count == 0) {
            value = default;
            return false;
        } else {
            value = PopBack();
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* PushFrontUninitialized() {
        EnsureCapacity(1);

        T* item = _ptr->Tail;

        if (item == _ptr->BufferEnd) item = _ptr->Buffer;

        _ptr->Count += 1;
        _ptr->Tail = &item[1];

        return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushFront(in T item) {
        T* ptr = PushFrontUninitialized();
        *ptr = item;
        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushFront(T item) => PushFront(in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T PopFront() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
        if (Count == 0) throw new InvalidOperationException("List is empty.");
#endif

        T* tail = _ptr->Tail;
        if (tail == _ptr->Buffer)
            tail = &_ptr->BufferEnd[-1];
        else
            tail = &tail[-1];

        --_ptr->Count;

        return *(_ptr->Tail = tail);
    }

    public bool TryPopFront(out T value) {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        if (Count == 0) {
            value = default;
            return false;
        } else {
            value = PopFront();
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(int index) {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
        if (index < 0 || index >= Count) throw new IndexOutOfRangeException(nameof(index));
#endif
        T* ptr = &_ptr->Head[index];

        if (ptr >= _ptr->BufferEnd) {
            nint offset = (nint)ptr - (nint)_ptr->BufferEnd;
            ptr = (T*)((nint)_ptr->Buffer + offset);
        }

        return ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index) => *GetPointer(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int index) => ref Unsafe.AsRef<T>(GetPointer(index));

    public ref T this[int index] {
        get => ref GetRef(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        _ptr->Count = 0;
        _ptr->Head = _ptr->Buffer;
        _ptr->Tail = _ptr->Buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(T item) {
        T* items = _ptr->Buffer;
        for (int i = 0; i < Count; i++)
            if (EqualityComparer<T>.Default.Equals(this[i], item))
                return i;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) {
        return IndexOf(item) != -1;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(this);

    public struct Enumerator : IEnumerator<T> {
        public readonly UnmanagedRingList<T> List;
        public int Index;

        public T Current {
            get {
#if EMBER_SAFETY_CHECKS
                if (Index < 0 || Index >= List.Count)
                    throw new IndexOutOfRangeException(nameof(Index));
#endif

                return List[Index];
            }
        }
        object IEnumerator.Current => Current;

        public Enumerator(UnmanagedRingList<T> list) {
#if EMBER_SAFETY_CHECKS
            if (list.IsNull) throw new NullReferenceException();
#endif
            List = list;
            Index = -1;
        }

        public bool MoveNext() {
            return ++Index != List.Count;
        }

        public void Reset() {
            Index = -1;
        }

        public void Dispose() { }
    }

#if DEBUG

    public void PrintDebug() {
        for (int i = 0; i < _ptr->Capacity; i++) {
            T* ptr = &_ptr->Buffer[i];

            Console.Write($"[{i}] = {_ptr->Buffer[i]}");

            if (ptr == _ptr->Head) Console.Write(" [HEAD]");
            if (ptr == &_ptr->Tail[-1]) Console.Write(" [TAIL]");
            Console.WriteLine();
        }
    }
#endif

}