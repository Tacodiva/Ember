
using System.Runtime.CompilerServices;

namespace Ember.Utils;

public readonly struct Bool32 {
    public static readonly Bool32 True = new(true);
    public static readonly Bool32 False = new(false);

    public readonly int IntValue;

    public bool Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IntValue != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bool32(bool value) {
        IntValue = value ? 1 : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(Bool32 bool32) => bool32.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Bool32(bool boolean) => new(boolean);
}