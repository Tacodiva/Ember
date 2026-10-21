
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Ember.Memory;

namespace Ember.Utils;

public static unsafe class UnsafeInterlocked {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Add<T>(T** location1, int value) where T : unmanaged =>
        (T*)Add((nint*)location1, value * sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Add(nint* location1, nint value) {
        return sizeof(nint) switch {
            4 => (nint)Add((int*)location1, (int)value),
            8 => (nint)Add((long*)location1, (long)value),
            _ => throw new UnreachableException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Add(nuint* location1, nuint value) {
        return sizeof(nuint) switch {
            4 => (nuint)Add((uint*)location1, (uint)value),
            8 => (nuint)Add((ulong*)location1, (ulong)value),
            _ => throw new UnreachableException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Add(scoped ref nint location1, nint value) {
        return sizeof(nint) switch {
            4 => (nint)Interlocked.Add(ref Unsafe.As<nint, int>(ref location1), (int)value),
            8 => (nint)Interlocked.Add(ref Unsafe.As<nint, long>(ref location1), (long)value),
            _ => throw new UnreachableException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Add(scoped ref nuint location1, nuint value) {
        return sizeof(nuint) switch {
            4 => (nuint)Interlocked.Add(ref Unsafe.As<nuint, uint>(ref location1), (uint)value),
            8 => (nuint)Interlocked.Add(ref Unsafe.As<nuint, ulong>(ref location1), (ulong)value),
            _ => throw new UnreachableException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Add(int* location1, int value) =>
        Interlocked.Add(ref Unsafe.AsRef<int>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Add(long* location1, long value) =>
        Interlocked.Add(ref Unsafe.AsRef<long>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Add(uint* location1, uint value) =>
        Interlocked.Add(ref Unsafe.AsRef<uint>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Add(ulong* location1, ulong value) =>
        Interlocked.Add(ref Unsafe.AsRef<ulong>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int And(int* location1, int value) =>
        Interlocked.And(ref Unsafe.AsRef<int>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long And(long* location1, long value) =>
        Interlocked.And(ref Unsafe.AsRef<long>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint And(uint* location1, uint value) =>
        Interlocked.And(ref Unsafe.AsRef<uint>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong And(ulong* location1, ulong value) =>
        Interlocked.And(ref Unsafe.AsRef<ulong>(location1), value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CompareExchange<T>(scoped ref T location1, T value, T comparand) {
        if (!typeof(T).IsValueType) {
            return Unsafe.BitCast<object, T>(Interlocked.CompareExchange(
                ref Unsafe.As<T, object>(ref location1),
                Unsafe.BitCast<T, object>(value),
                Unsafe.BitCast<T, object>(comparand)
            ));
        }

        return Unsafe.SizeOf<T>() switch {
            4 => Unsafe.BitCast<int, T>(Interlocked.CompareExchange(
                ref Unsafe.As<T, int>(ref location1),
                Unsafe.BitCast<T, int>(value),
                Unsafe.BitCast<T, int>(comparand)
            )),
            8 => Unsafe.BitCast<long, T>(Interlocked.CompareExchange(
                ref Unsafe.As<T, long>(ref location1),
                Unsafe.BitCast<T, long>(value),
                Unsafe.BitCast<T, long>(comparand)
            )),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CompareExchange<T>(T* location1, T value, T comparand) where T : unmanaged
        => CompareExchange(ref Unsafe.AsRef<T>(location1), value, comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* CompareExchange<T>(T** location1, T* value, T* comparand) where T : unmanaged =>
        (T*)Interlocked.CompareExchange(ref Unsafe.AsRef<nint>(location1), (nint)value, (nint)comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* CompareExchange<T>(scoped ref T* location1, T* value, T* comparand) where T : unmanaged =>
        (T*)Interlocked.CompareExchange(ref UnsafeUtils.AsNIntRef(ref location1), (nint)value, (nint)comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompareExchange<T>(scoped ref T location1, T value, T comparand) {
        if (!typeof(T).IsValueType) {
            return ReferenceEquals(Interlocked.CompareExchange(
                ref Unsafe.As<T, object>(ref location1),
                Unsafe.BitCast<T, object>(value),
                Unsafe.BitCast<T, object>(comparand)
            ), Unsafe.BitCast<T, object>(comparand));
        }

        return Unsafe.SizeOf<T>() switch {
            4 => Interlocked.CompareExchange(
                ref Unsafe.As<T, int>(ref location1),
                Unsafe.BitCast<T, int>(value),
                Unsafe.BitCast<T, int>(comparand)
            ) == Unsafe.BitCast<T, int>(comparand),
            8 => Interlocked.CompareExchange(
                ref Unsafe.As<T, long>(ref location1),
                Unsafe.BitCast<T, long>(value),
                Unsafe.BitCast<T, long>(comparand)
            ) == Unsafe.BitCast<T, long>(comparand),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompareExchange<T>(T* location1, T value, T comparand) where T : unmanaged
        => TryCompareExchange(ref Unsafe.AsRef<T>(location1), value, comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompareExchange<T>(T** location1, T* value, T* comparand) where T : unmanaged =>
        Interlocked.CompareExchange(ref Unsafe.AsRef<nint>(location1), (nint)value, (nint)comparand) == (nint)comparand;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompareExchange<T>(scoped ref T* location1, T* value, T* comparand) where T : unmanaged =>
        Interlocked.CompareExchange(ref UnsafeUtils.AsNIntRef(ref location1), (nint)value, (nint)comparand) == (nint)comparand;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Decrement(int* location) =>
        Interlocked.Decrement(ref Unsafe.AsRef<int>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Decrement(long* location) =>
        Interlocked.Decrement(ref Unsafe.AsRef<long>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Decrement(uint* location) =>
        Interlocked.Decrement(ref Unsafe.AsRef<uint>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Decrement(ulong* location) =>
        Interlocked.Decrement(ref Unsafe.AsRef<ulong>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exchange<T>(scoped ref T location1, T comparand) {
        if (!typeof(T).IsValueType) {
            return Unsafe.BitCast<object, T>(Interlocked.Exchange(
                ref Unsafe.As<T, object>(ref location1),
                Unsafe.BitCast<T, object>(comparand)
            ));
        }

        return Unsafe.SizeOf<T>() switch {
            4 => Unsafe.BitCast<int, T>(Interlocked.Exchange(
                ref Unsafe.As<T, int>(ref location1),
                Unsafe.BitCast<T, int>(comparand)
            )),
            8 => Unsafe.BitCast<long, T>(Interlocked.Exchange(
                ref Unsafe.As<T, long>(ref location1),
                Unsafe.BitCast<T, long>(comparand)
            )),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exchange<T>(T* location1, T comparand) where T : unmanaged =>
        Exchange(ref Unsafe.AsRef<T>(location1), comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Exchange<T>(T** location1, T* value) where T : unmanaged =>
        (T*)Interlocked.Exchange(ref Unsafe.AsRef<nint>(location1), (nint)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Exchange<T>(scoped ref T* location1, T* value) where T : unmanaged =>
        (T*)Interlocked.Exchange(ref UnsafeUtils.AsNIntRef(ref location1), (nint)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(scoped ref readonly T location) {
        if (!typeof(T).IsValueType) {
            return Unsafe.BitCast<object, T>(Volatile.Read(ref Unsafe.As<T, object>(ref Unsafe.AsRef(in location))));
        }

        switch (Unsafe.SizeOf<T>()) {
            case 4:
                return Unsafe.BitCast<int, T>(Volatile.Read(ref Unsafe.As<T, int>(ref Unsafe.AsRef(in location))));
            case 8:
                if (sizeof(nint) < 8) {
                    return Unsafe.BitCast<long, T>(Interlocked.Read(ref Unsafe.As<T, long>(ref Unsafe.AsRef(in location))));
                } else {
                    return Unsafe.BitCast<long, T>(Volatile.Read(ref Unsafe.As<T, long>(ref Unsafe.AsRef(in location))));
                }

            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(T* location) where T : unmanaged =>
        Read(ref Unsafe.AsRef<T>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Read<T>(T** location) where T : unmanaged =>
        (T*)Read(ref Unsafe.AsRef<nint>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Read<T>(scoped ref readonly T* location) where T : unmanaged =>
        (T*)Read(in UnsafeUtils.AsNIntRefReadonly(in location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Increment(int* location) =>
        Interlocked.Increment(ref Unsafe.AsRef<int>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Increment(long* location) =>
        Interlocked.Increment(ref Unsafe.AsRef<long>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Increment(uint* location) =>
        Interlocked.Increment(ref Unsafe.AsRef<uint>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Increment(ulong* location) =>
        Interlocked.Increment(ref Unsafe.AsRef<ulong>(location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Or(int* location1, int value) =>
        Interlocked.Or(ref Unsafe.AsRef<int>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Or(long* location1, long value) =>
        Interlocked.Or(ref Unsafe.AsRef<long>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Or(uint* location1, uint value) =>
        Interlocked.Or(ref Unsafe.AsRef<uint>(location1), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Or(ulong* location1, ulong value) =>
        Interlocked.Or(ref Unsafe.AsRef<ulong>(location1), value);

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
                if (sizeof(nint) < 8) {
                    Interlocked.Exchange(ref Unsafe.As<T, long>(ref addr), Unsafe.As<T, long>(ref value));
                } else {
                    Volatile.Write(ref Unsafe.As<T, long>(ref addr), Unsafe.As<T, long>(ref value));
                }
                break;
            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(T* addr, T value) where T : unmanaged =>
        Write(ref Unsafe.AsRef<T>(addr), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(T** addr, T* value) where T : unmanaged =>
        Write(ref Unsafe.AsRef<nint>(addr), (nint)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(scoped ref T* addr, T* value) where T : unmanaged =>
        Write(ref UnsafeUtils.AsNIntRef(ref addr), (nint)value);

}