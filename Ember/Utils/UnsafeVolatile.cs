
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ember.Utils;

public unsafe static class UnsafeVolatile {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(scoped ref readonly T addr) {
        if (!typeof(T).IsValueType) {
            return Unsafe.BitCast<object, T>(Volatile.Read(ref Unsafe.As<T, object>(ref Unsafe.AsRef(in addr))));
        }

        return Unsafe.SizeOf<T>() switch {
            4 => Unsafe.BitCast<int, T>(Volatile.Read(ref Unsafe.As<T, int>(ref Unsafe.AsRef(in addr)))),
            8 => Unsafe.BitCast<long, T>(Volatile.Read(ref Unsafe.As<T, long>(ref Unsafe.AsRef(in addr)))),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(T* addr) where T : unmanaged
        => Read(ref Unsafe.AsRef<T>(addr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Read<T>(scoped ref readonly T* addr) where T : unmanaged
        => (T*)Volatile.Read(in UnsafeUtils.AsNIntRefReadonly(in addr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(scoped ref T addr, T value) {
        if (!typeof(T).IsValueType) {
            Volatile.Write(ref Unsafe.As<T, object>(ref addr), Unsafe.BitCast<T, object>(value));
            return;
        }

        switch (Unsafe.SizeOf<T>()) {
            case 4:
                Volatile.Write(ref Unsafe.As<T, int>(ref addr), Unsafe.As<T, int>(ref value));
                break;
            case 8:
                Volatile.Write(ref Unsafe.As<T, long>(ref addr), Unsafe.As<T, long>(ref value));
                break;
            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(T* addr, T value) where T : unmanaged
        => Write(ref Unsafe.AsRef<T>(addr), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(scoped ref T* addr, T* value) where T : unmanaged
        => Volatile.Write(ref UnsafeUtils.AsNIntRef(ref addr), (nint)value);
}