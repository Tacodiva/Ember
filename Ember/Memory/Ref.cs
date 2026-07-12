
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
    public bool Equals(Ref<T> other)
        => Unsafe.AreSame(ref Value, ref other.Value);

    public ref T this[nint i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(ref Value, i);
    }
}