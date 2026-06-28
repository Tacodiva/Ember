
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ember.Memory;

public unsafe readonly struct MemorySpan : IMemorySpan {
    public readonly void* Pointer;
    public readonly nint LengthLong;
    public int Length => (int)LengthLong;

    public bool IsNull => Pointer == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan(void* pointer, nint length) {
        Pointer = pointer;
        LengthLong = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    MemorySpan IMemorySpan.AsMemorySpan() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AlignedMemorySpan<T> AsAlignedSpan<T>(int stride) where T : unmanaged {
        if (LengthLong % stride != 0)
            throw new ArgumentException($"MemorySpan's length {LengthLong} not divisible by stride ({stride}).");
        return new AlignedMemorySpan<T>(Pointer, stride, (int)(LengthLong / stride));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan<T>(nint offsetBytes = 0) where T : unmanaged {
        return AsSpan<T>(offsetBytes, LengthLong - offsetBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan<T>(nint offsetBytes, nint lengthBytes) where T : unmanaged {
        return AsMemorySpan<T>(offsetBytes, lengthBytes).AsSpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> AsMemorySpan<T>(nint offsetBytes = 0) where T : unmanaged {
        return AsMemorySpan<T>(offsetBytes, LengthLong - offsetBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> AsMemorySpan<T>(nint offsetBytes, nint lengthBytes) where T : unmanaged {
        if (lengthBytes > LengthLong - offsetBytes || lengthBytes < 0 || offsetBytes < 0)
            throw new ArgumentOutOfRangeException();
        if (lengthBytes % sizeof(T) != 0)
            throw new ArgumentException($"MemorySpan's length {lengthBytes} not divisible by size of T ({sizeof(T)}).");
        return new MemorySpan<T>((T*)((nint)Pointer + offsetBytes), lengthBytes / sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void* AsPointer() {
        return Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* AsPointer<T>() where T : unmanaged {
        if (LengthLong != sizeof(T))
            throw new InvalidOperationException($"Length of span {LengthLong} does not match length of T {sizeof(T)}.");
        return (T*)Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T AsRef<T>() where T : unmanaged {
        if (LengthLong != sizeof(T))
            throw new InvalidOperationException($"Length of span {LengthLong} does not match length of T {sizeof(T)}.");
        return ref Unsafe.AsRef<T>(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan Slice(nint offset) {
        return new MemorySpan((byte*)Pointer + offset, LengthLong - offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan Slice(nint offset, nint length) {
        if (length > LengthLong - offset || length < 0 || offset < 0)
            throw new ArgumentOutOfRangeException();
        return new MemorySpan((byte*)Pointer + offset, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MemorySpan, MemorySpan) Bisect(nint offset) {
        if (offset < 0 || offset > LengthLong) throw new ArgumentOutOfRangeException(nameof(offset));
        return (
            new MemorySpan(Pointer, offset),
            new MemorySpan((byte*)Pointer + offset, LengthLong - offset)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(MemorySpan other) {
        if (LengthLong != other.LengthLong) throw new InvalidOperationException("Memory not the same length.");
        MemoryUtils.Copy(Pointer, other.Pointer, (nuint)LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(byte value = 0) {
        MemoryUtils.Fill(Pointer, (nuint)LengthLong, value);
    }

    public bool MemoryEquals(MemorySpan other) {
        if (LengthLong != other.LengthLong) return false;
        if (Pointer == other.Pointer) return true;

        return MemoryUtils.Compare(Pointer, other.Pointer, (nuint)LengthLong);
    }
}

public unsafe readonly struct MemorySpan<T> : IMemorySpan<T>, IEnumerable<T> where T : unmanaged {
    public readonly T* Pointer;
    public readonly nint LengthLong;
    public int Length => (int)LengthLong;

    public bool IsNull => Pointer == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan(T* pointer, nint length) {
        Pointer = pointer;
        LengthLong = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan(Span<T> span) {
        LengthLong = span.Length;
        fixed (T* pointer = span) Pointer = pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    MemorySpan<T> IMemorySpan<T>.AsMemorySpan() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan AsMemorySpan() {
        return new MemorySpan(Pointer, LengthLong * sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AlignedMemorySpan<T> AsAlignedSpan() {
        return new AlignedMemorySpan<T>(Pointer, sizeof(T), LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(LengthLong, int.MaxValue, nameof(LengthLong));
        return new Span<T>(Pointer, (int)LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(nint index) {
        return ref Unsafe.AsRef<T>(GetPointer(index));
    }

    public ref T AsRef() {
        return ref Unsafe.AsRef<T>(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(nint index, in T value) {
        *GetPointer(index) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(nint index, T* value) {
        *GetPointer(index) = *value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(nint index) {
        if (index < 0 || index >= LengthLong) throw new ArgumentOutOfRangeException(nameof(index));
        return &Pointer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> Slice(nint start) {
        return Slice(start, LengthLong - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySpan<T> Slice(nint start, nint length) {
        if (start >= LengthLong || start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (LengthLong - start < length || length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        return new MemorySpan<T>(GetPointer(start), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MemorySpan<T>, MemorySpan<T>) Bisect(nint offset) {
        if (offset < 0 || offset > LengthLong) throw new ArgumentOutOfRangeException(nameof(offset));
        return (
            new MemorySpan<T>(Pointer, offset),
            new MemorySpan<T>(&Pointer[offset], LengthLong - offset)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(MemorySpan<T> other) {
        if (LengthLong != other.LengthLong) throw new InvalidOperationException("Memory not the same length.");
        MemoryUtils.Copy(Pointer, other.Pointer, (nuint)LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> other) {
        if (LengthLong != other.Length) throw new InvalidOperationException("Memory not the same length.");
        fixed (T* pOther = other)
            MemoryUtils.Copy(Pointer, pOther, (nuint)LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(AlignedMemorySpan<T> other) {
        if (other.Stride == sizeof(T)) {
            CopyTo(other.AsMemorySpan().AsSpan<T>());
        } else {
            if (LengthLong != other.LengthLong) throw new InvalidOperationException("Memory not the same length.");
            for (int i = 0; i < LengthLong; i++)
                other[i] = this[i];
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        if (array.Length - arrayIndex < Length)
            throw new ArgumentException("The destination array is too small.");

        fixed (T* arrayPtr = &array[arrayIndex])
            MemoryUtils.Copy<T>(Pointer, arrayPtr, Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(ReadOnlySpan<T> src) {
        if (LengthLong != src.Length) throw new InvalidOperationException("Memory not the same length.");
        fixed (T* pSrc = src)
            MemoryUtils.Copy(pSrc, Pointer, (nuint)LengthLong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InitBlock(byte value = 0) {
        AsMemorySpan().Fill(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort() => AsSpan().Sort();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<T> => AsSpan().Sort(comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort(IComparer<T> comparer) => AsSpan().Sort(comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort(Comparison<T> comparer) => AsSpan().Sort(comparer);

    public ref T this[nint i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Get(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray() {
        T[] array = new T[LengthLong];
        fixed (T* arrayPtr = array)
            MemoryUtils.Copy(Pointer, arrayPtr, (nuint)LengthLong);
        return array;
    }

    public bool MemoryEqual(MemorySpan<T> other) {
        return AsMemorySpan().MemoryEquals(other.AsMemorySpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(MemorySpan<T> memorySpan) => memorySpan.AsSpan();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(MemorySpan<T> memorySpan) => memorySpan.AsSpan();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator MemorySpan<T>(Span<T> span) => new(span);

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        public readonly T Current => Span[Index];
        readonly object IEnumerator.Current => Current;

        public readonly MemorySpan<T> Span;
        public nint Index;

        public Enumerator(MemorySpan<T> span) {
            Span = span;
            Reset();
        }

        public bool MoveNext() {
            return ++Index < Span.LengthLong;
        }

        public void Reset() {
            Index = -1;
        }

        public readonly void Dispose() { }
    }
}