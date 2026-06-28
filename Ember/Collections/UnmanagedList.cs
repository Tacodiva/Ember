
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


public unsafe readonly struct UnmanagedList<T>(UnmanagedInlineList<T>* list) :
    IList<T>, IPtrWrapper<UnmanagedInlineList<T>>, IReadOnlyList<T>, IDisposable
    where T : unmanaged {

    public static UnmanagedList<T> Null => default;

    public static UnmanagedList<T> Allocate(int capacity = 16) {
        UnmanagedInlineList<T>* inlineList = MemoryUtils.AllocateUninitialized<UnmanagedInlineList<T>>();
        inlineList->Init(capacity);
        return new UnmanagedList<T>(inlineList);
    }

    public static UnmanagedList<T> Allocate(ReadOnlySpan<T> contents) {
        UnmanagedInlineList<T>* inlineList = MemoryUtils.AllocateUninitialized<UnmanagedInlineList<T>>();
        inlineList->Init(contents);
        return new UnmanagedList<T>(inlineList);
    }

    private readonly UnmanagedInlineList<T>* _list = list;

    public UnmanagedInlineList<T>* Pointer => _list;

    public int Count {
        get {
            ValidateSelf();
            return _list->Count;
        }
    }

    public bool IsReadOnly => false;
    public bool IsEmpty => Count == 0;
    public bool IsNull => _list == null;

    public T* BasePointer {
        get {
            ValidateSelf();
            return _list->BasePointer;
        }
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal void ValidateSelf() {
        if (IsNull) throw new NullReferenceException($"UnmanagedList<{typeof(T)}> is null.");
    }

    public void Dispose() {
        ValidateSelf();
        _list->Dispose();
        MemoryUtils.Free(_list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* AddUninitialized() {
        ValidateSelf();
        return _list->AddUninitialized();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> AddUninitialized(int count) {
        ValidateSelf();
        return _list->AddUninitialized(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T item) {
        ValidateSelf();
        _list->Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => Add(in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddAll(MemorySpan<T> items) {
        ValidateSelf();
        _list->AddAll(items);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop() {
        ValidateSelf();
        return _list->Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Pop(int amount) {
        ValidateSelf();
        _list->Pop(amount);
    }

    public bool TryPop(out T value) {
        ValidateSelf();
        return _list->TryPop(out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, in T item) {
        ValidateSelf();
        _list->Insert(index, item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item) => Insert(index, in item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(int index) {
        ValidateSelf();
        return _list->GetPointer(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index) => *GetPointer(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int index) => ref Unsafe.AsRef<T>(GetPointer(index));

    public T this[int index] {
        get => Get(index);
        set {
            *GetPointer(index) = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        ValidateSelf();
        _list->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(T item) {
        ValidateSelf();
        return _list->IndexOf(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        ValidateSelf();
        _list->CopyTo(array, arrayIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index) {
        ValidateSelf();
        _list->RemoveAt(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item) {
        ValidateSelf();
        return _list->Remove(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) {
        return IndexOf(item) != -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> GetMemorySpan() {
        ValidateSelf();
        return _list->GetMemorySpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort() => GetMemorySpan().Sort();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<T> => GetMemorySpan().Sort(comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort(IComparer<T> comparer) => GetMemorySpan().Sort(comparer);

    public UnmanagedInlineList<T>.Enumerator GetEnumerator() {
        ValidateSelf();
        return new(*_list);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public static implicit operator UnmanagedInlineList<T>*(UnmanagedList<T> list) => list._list;
    public static implicit operator UnmanagedList<T>(UnmanagedInlineList<T>* list) => new(list);

}