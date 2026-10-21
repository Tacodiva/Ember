
using System.Runtime.CompilerServices;

namespace Ember.Memory;

#pragma warning disable CS0660
#pragma warning disable CS0661

public readonly ref struct Ref<T>(ref T reference) {

    public readonly ref T Value = ref reference;

    public bool IsNull => Unsafe.IsNullRef(ref Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ref<T> left, Ref<T> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ref<T> left, Ref<T> right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RefReadonly<T>(Ref<T> us) => new(ref us.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Ref<T> other)
        => Unsafe.AreSame(ref Value, ref other.Value);

    public ref T this[nint i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(ref Value, i);
    }
}

public readonly ref struct RefReadonly<T>(ref readonly T reference) {

    public readonly ref readonly T Value = ref reference;

    public bool IsNull => Unsafe.IsNullRef(in Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RefReadonly<T> left, RefReadonly<T> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RefReadonly<T> left, RefReadonly<T> right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ref<T> AsRef() => new(ref Unsafe.AsRef(in Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(RefReadonly<T> other)
        => Unsafe.AreSame(in Value, in other.Value);

    public ref readonly T this[nint i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(ref Unsafe.AsRef(in Value), i);
    }
}