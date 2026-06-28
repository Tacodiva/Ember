
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ember.Memory;


public unsafe readonly struct AlignedMemorySpan<T> : IEnumerable<T> where T : unmanaged {

    public static int GetStide(int alignment) {
        if ((alignment & (alignment - 1)) != 0)
            throw new ArgumentException($"Expected {nameof(alignment)} to be a power of 2. Got {alignment}.");

        return (sizeof(T) & ~(alignment - 1)) + alignment;
    }

    public static AlignedMemorySpan<T> FromFixedSpan(Span<T> span) {
        fixed (T* spanPtr = span)
            return new(spanPtr, sizeof(T), span.Length);
    }

    public readonly void* Pointer;
    public readonly nint LengthLong;
    public readonly int Stride;

    public int Length => (int)LengthLong;

    public bool IsNull => Pointer == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AlignedMemorySpan(void* pointer, int stride, nint length) {
        ArgumentOutOfRangeException.ThrowIfLessThan(stride, sizeof(T));
        Pointer = pointer;
        LengthLong = length;
        Stride = stride;
    }

    public MemorySpan AsMemorySpan() {
        return new MemorySpan(Pointer, LengthLong * Stride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(nint index) {
        return ref Unsafe.AsRef<T>(GetPointer(index));
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
        return (T*)((nint)Pointer + index * Stride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AlignedMemorySpan<T> Slice(nint start) {
        return Slice(start, LengthLong - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AlignedMemorySpan<T> Slice(nint start, nint length) {
        if (start > LengthLong || start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (LengthLong - start < length || length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        return new AlignedMemorySpan<T>(GetPointer(start), Stride, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (AlignedMemorySpan<T>, AlignedMemorySpan<T>) Bisect(nint offset) {
        if (offset < 0 || offset > LengthLong) throw new ArgumentOutOfRangeException(nameof(offset));
        return (
            new AlignedMemorySpan<T>(Pointer, Stride, offset),
            new AlignedMemorySpan<T>(GetPointer(offset), Stride, LengthLong - offset)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(AlignedMemorySpan<T> other) {
        if (LengthLong != other.LengthLong) throw new InvalidOperationException("Memory not the same length.");
        if (Stride == other.Stride) {
            AsMemorySpan().CopyTo(other.AsMemorySpan());
        } else {
            for (int i = 0; i < LengthLong; i++) {
                other[i] = this[i];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> other) {
        if (LengthLong != other.Length) throw new InvalidOperationException("Memory not the same length.");
        if (Stride == sizeof(T)) {
            AsMemorySpan().AsSpan<T>().CopyTo(other);
        } else {
            for (int i = 0; i < LengthLong; i++) {
                other[i] = this[i];
            }
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        if (array.Length - arrayIndex < Length)
            throw new ArgumentException("The destination array is too small.");

        for (int i = 0; i < Length; i++)
            array[i + arrayIndex] = this[i];
    }

    public ref T this[nint i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Get(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray() {
        T[] array = new T[LengthLong];
        CopyTo(array.AsSpan());
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AlignedMemorySpan<T>(MemorySpan<T> span) => new(span.Pointer, sizeof(T), span.LengthLong);

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        public readonly T Current => Span[Index];
        readonly object IEnumerator.Current => Current;

        public readonly AlignedMemorySpan<T> Span;
        public nint Index;

        public Enumerator(AlignedMemorySpan<T> span) {
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