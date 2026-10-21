
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ember.Utils;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public unsafe struct SlimSpinLock {

    #region Utilities 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilEqual<T>(scoped ref readonly T valueRef, T comparand) {
        if (!typeof(T).IsValueType) {
            SpinUntilEqual(ref Unsafe.As<T, object>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, object>(comparand));
            return;
        }

        switch (Unsafe.SizeOf<T>()) {
            case 4:
                SpinUntilEqual(ref Unsafe.As<T, int>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, int>(comparand));
                return;
            case 8:
                SpinUntilEqual(ref Unsafe.As<T, long>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, long>(comparand));
                return;
            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilEqual<T>(T* valueRef, T comparand) where T : unmanaged
        => SpinUntilEqual(ref Unsafe.AsRef<T>(valueRef), comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilEqual(scoped ref readonly int valueRef, int comparand) {
        if (Volatile.Read(in valueRef) == comparand) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (Volatile.Read(in valueRef) != comparand);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilEqual(scoped ref readonly long valueRef, long comparand) {
        if (Interlocked.Read(in valueRef) == comparand) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (Interlocked.Read(in valueRef) != comparand);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilEqual(scoped ref readonly object valueRef, object comparand) {
        if (ReferenceEquals(Volatile.Read(in valueRef), comparand)) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (!ReferenceEquals(Volatile.Read(in valueRef), comparand));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpinUntilNotEqual<T>(scoped ref readonly T valueRef, T comparand) {
        if (!typeof(T).IsValueType) {
            return Unsafe.BitCast<object, T>(
                SpinUntilNotEqual(ref Unsafe.As<T, object>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, object>(comparand))
            );
        }

        return Unsafe.SizeOf<T>() switch {
            4 => Unsafe.BitCast<int, T>(SpinUntilNotEqual(ref Unsafe.As<T, int>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, int>(comparand))),
            8 => Unsafe.BitCast<long, T>(SpinUntilNotEqual(ref Unsafe.As<T, long>(ref Unsafe.AsRef(in valueRef)), Unsafe.BitCast<T, long>(comparand))),
            _ => throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpinUntilNotEqual<T>(T* valueRef, T comparand) where T : unmanaged
        => SpinUntilNotEqual(ref Unsafe.AsRef<T>(valueRef), comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SpinUntilNotEqual(scoped ref readonly int valueRef, int comparand) {
        int value = Volatile.Read(in valueRef);
        if (value != comparand) return value;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
            value = Volatile.Read(in valueRef);
        } while (value == comparand);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long SpinUntilNotEqual(scoped ref readonly long valueRef, long comparand) {
        long value = Interlocked.Read(in valueRef);
        if (value != comparand) return value;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
            value = Interlocked.Read(in valueRef);
        } while (value == comparand);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object SpinUntilNotEqual(scoped ref readonly object valueRef, object comparand) {
        object value = Volatile.Read(in valueRef);
        if (!ReferenceEquals(value, comparand)) return value;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
            value = Volatile.Read(in valueRef);
        } while (ReferenceEquals(value, comparand));
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilCompareExchange<T>(scoped ref T valueRef, T value, T comparand) {
        if (!typeof(T).IsValueType) {
            SpinUntilCompareExchange(ref Unsafe.As<T, object>(ref valueRef), Unsafe.BitCast<T, object>(value), Unsafe.BitCast<T, object>(comparand));
            return;
        }
        
        switch (Unsafe.SizeOf<T>()) {
            case 4:
                SpinUntilCompareExchange(ref Unsafe.As<T, int>(ref valueRef), Unsafe.BitCast<T, int>(value), Unsafe.BitCast<T, int>(comparand));
                return;
            case 8:
                SpinUntilCompareExchange(ref Unsafe.As<T, long>(ref valueRef), Unsafe.BitCast<T, long>(value), Unsafe.BitCast<T, long>(comparand));
                return;
            default:
                throw new InvalidOperationException($"Only values with a size of 4 or 8 are supported. Got {Unsafe.SizeOf<T>()}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilCompareExchange<T>(T* valueRef, T value, T comparand) where T : unmanaged
        => SpinUntilCompareExchange(ref Unsafe.AsRef<T>(valueRef), value, comparand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilCompareExchange(scoped ref int valueRef, int value, int comparand) {
        if (Interlocked.CompareExchange(ref valueRef, value, comparand) == comparand) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (Interlocked.CompareExchange(ref valueRef, value, comparand) != comparand);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilCompareExchange(scoped ref long valueRef, long value, long comparand) {
        if (Interlocked.CompareExchange(ref valueRef, value, comparand) == comparand) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (Interlocked.CompareExchange(ref valueRef, value, comparand) != comparand);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinUntilCompareExchange(scoped ref object valueRef, object value, object comparand) {
        if (ReferenceEquals(Interlocked.CompareExchange(ref valueRef, value, comparand), comparand)) return;
        SpinWait sw = new();
        do {
            sw.SpinOnce();
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref valueRef, value, comparand), comparand));
    }
    #endregion

    #region Lock / Unlock

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HeldRefSlimSpinLock Acquire(ref SlimSpinLock lockRef) {
        SpinUntilCompareExchange(ref lockRef, Acquired, Released);
        return new(ref lockRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HeldPtrSlimSpinLock Acquire(SlimSpinLock* lockPointer) {
        SpinUntilCompareExchange(lockPointer, Acquired, Released);
        return new(lockPointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Release(scoped ref SlimSpinLock lockRef) {
#if EMBER_SAFETY_CHECKS
        if (!lockRef.IsAcquired)
            throw new InvalidOperationException("Lock is not acquired.");
#endif
        UnsafeVolatile.Write(ref lockRef, Released);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Release(SlimSpinLock* lockPointer) {
#if EMBER_SAFETY_CHECKS
        if (!lockPointer->IsAcquired)
            throw new InvalidOperationException("Lock is not acquired.");
#endif
        UnsafeVolatile.Write(lockPointer, Released);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquire(ref SlimSpinLock lockRef, out HeldRefSlimSpinLock referenceLock) {
        if (UnsafeInterlocked.TryCompareExchange(ref lockRef, Acquired, Released)) {
            referenceLock = new(ref lockRef);
            return true;
        }

        referenceLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquire(SlimSpinLock* lockRef, out HeldPtrSlimSpinLock ptrLock) {
        if (UnsafeInterlocked.TryCompareExchange(lockRef, Acquired, Released)) {
            ptrLock = new(lockRef);
            return true;
        }

        ptrLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRelease(scoped ref SlimSpinLock lockRef) {
        return UnsafeInterlocked.Exchange(ref lockRef, Released).IsAcquired;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRelease(SlimSpinLock* lockPointer) {
        return UnsafeInterlocked.Exchange(lockPointer, Released).IsAcquired;
    }

    #endregion

    public static SlimSpinLock Acquired {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.BitCast<int, SlimSpinLock>(1);
    }

    public static SlimSpinLock Released {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private int _value;
#pragma warning restore IDE0044 // Add readonly modifier

    public readonly bool IsAcquired {
        // This could be (_value != 0), but technically it should be read as a volatile so we get this
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeInterlocked.Read(in this)._value != 0;
    }
}

public readonly ref struct HeldRefSlimSpinLock {
    private readonly ref SlimSpinLock _lock;
    public readonly bool IsNull => Unsafe.IsNullRef(ref _lock);
    public ref SlimSpinLock Lock => ref _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldRefSlimSpinLock(ref SlimSpinLock lockRef) {
        _lock = ref lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) SlimSpinLock.Release(ref _lock);
    }
}

public readonly unsafe struct HeldPtrSlimSpinLock : IDisposable {
    private readonly SlimSpinLock* _lock;
    public readonly bool IsNull => _lock == null;
    public SlimSpinLock* Lock => _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldPtrSlimSpinLock(SlimSpinLock* lockPointer) {
        _lock = lockPointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (_lock != null) SlimSpinLock.Release(_lock);
    }
}
