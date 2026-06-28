
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ember.Utils;

public unsafe static class UnsafeVolatile {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ref T addr) where T : unmanaged {
        return sizeof(T) switch {
            4 => Unsafe.BitCast<int, T>(Volatile.Read(ref Unsafe.As<T, int>(ref addr))),
            8 => Unsafe.BitCast<long, T>(Volatile.Read(ref Unsafe.As<T, long>(ref addr))),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {sizeof(T)}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref T addr, T value) where T : unmanaged {
        switch (sizeof(T)) {
            case 4:
                Volatile.Write(ref Unsafe.As<T, int>(ref addr), Unsafe.As<T, int>(ref value));
                break;
            case 8:
                Volatile.Write(ref Unsafe.As<T, long>(ref addr), Unsafe.As<T, long>(ref value));
                break;
            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {sizeof(T)}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Read<T>(ref T* addr) where T : unmanaged
        => (T*)Volatile.Read(ref UnsafeUtils.AsNIntRef(ref addr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref T* addr, T* value) where T : unmanaged
        => Volatile.Write(ref UnsafeUtils.AsNIntRef(ref addr), (nint)value);
}