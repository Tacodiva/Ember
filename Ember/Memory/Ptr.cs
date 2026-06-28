
using System;
using System.Runtime.CompilerServices;
using Ember.Utils;

namespace Ember.Memory;

public unsafe readonly struct Ptr : IDisposable, IEquatable<Ptr> {

    public readonly void* Pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ptr(void* ptr) {
        Pointer = ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free() {
        MemoryUtils.Free(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        Free();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ptr<T> AsPtr<T>() where T : unmanaged {
        return new Ptr<T>((T*)Pointer);
    }

    public static bool operator ==(Ptr left, Ptr right) => Equals(left, right);

    public static bool operator !=(Ptr left, Ptr right) => !Equals(left, right);

    public override bool Equals(object? obj) {
        return obj is Ptr ptr && Equals(ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Ptr other) {
        return other.Pointer == Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() {
        return ((nint)Pointer).GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(Ptr ptr) => (nint)ptr.Pointer;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr(nint ptr) => new((void*)ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void*(Ptr ptr) => ptr.Pointer;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr(void* ptr) => new(ptr);
}

public unsafe readonly struct Ptr<T> : IDisposable, IEquatable<Ptr<T>> where T : unmanaged {

    public readonly T* Pointer;

    public readonly bool IsNull => Pointer == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ptr<T> Allocate() {
        return new Ptr<T>(MemoryUtils.Allocate<T>(1, 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ptr<T> AllocateUninitialized() {
        return new Ptr<T>(MemoryUtils.AllocateUninitialized<T>(1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ptr(T* ptr) {
        Pointer = ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(T value) {
        *Pointer = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get() {
        return *Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T AsRef() {
        return ref Unsafe.AsRef<T>(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free() {
        MemoryUtils.Free(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        Free();
    }

    public static bool operator ==(Ptr<T> left, Ptr<T> right) => Equals(left, right);

    public static bool operator !=(Ptr<T> left, Ptr<T> right) => !Equals(left, right);

    public override bool Equals(object? obj) {
        return obj is Ptr<T> ptr && Equals(ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Ptr<T> other) {
        return other.Pointer == Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() {
        return ((nint)Pointer).GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(Ptr<T> ptr) => (nint)ptr.Pointer;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr<T>(nint ptr) => new((T*)ptr);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(Ptr<T> ptr) => ptr.Pointer;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr<T>(T* ptr) => new(ptr);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr(Ptr<T> ptr) => new(ptr.Pointer);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void*(Ptr<T> ptr) => ptr.Pointer;

}