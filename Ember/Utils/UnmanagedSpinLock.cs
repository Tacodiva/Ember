
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ember.Utils;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct SpinLockValue {
    private readonly int _value;

    // This could be (_value != 0), but technically it should be read as a volatile so we get this
    public bool IsAquired => Volatile.Read(ref Unsafe.As<SpinLockValue, int>(ref this)) != 0;
}

public readonly unsafe struct UnmanagedSpinLock : IDisposable {
    public static UnmanagedSpinLock Aquire(SpinLockValue* lockPointer) {
        SpinWait spinWait = new();
        while (UnsafeInterlocked.CompareExchange((int*)lockPointer, 1, 0) != 0)
            spinWait.SpinOnce();
        return new(lockPointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Release(SpinLockValue* lockPointer) {
        Debug.Assert(lockPointer->IsAquired);
        *(int*)lockPointer = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRelease(SpinLockValue* lockPointer) {
        return UnsafeInterlocked.Exchange((int*)lockPointer, 0) == 1;
    }


    private readonly SpinLockValue* _lockPtr;

    public readonly bool IsNull => _lockPtr == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UnmanagedSpinLock(SpinLockValue* lockPointer) {
        _lockPtr = lockPointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (_lockPtr != null) Release(_lockPtr);
    }
}

public readonly unsafe ref struct ReferenceSpinLock {

    public static ReferenceSpinLock Aquire(ref SpinLockValue lockRef) {
        SpinWait spinWait = new();
        while (Interlocked.CompareExchange(ref Unsafe.As<SpinLockValue, int>(ref lockRef), 1, 0) != 0)
            spinWait.SpinOnce();
        return new(ref lockRef);
    }

    public static bool TryAquire(ref SpinLockValue lockRef, out ReferenceSpinLock referenceLock) {
        if (Interlocked.CompareExchange(ref Unsafe.As<SpinLockValue, int>(ref lockRef), 1, 0) == 0) {
            referenceLock = new(ref lockRef);
            return true;
        }

        referenceLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Release(ref SpinLockValue lockRef) {
        Debug.Assert(lockRef.IsAquired);
        Unsafe.As<SpinLockValue, int>(ref lockRef) = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRelease(ref SpinLockValue lockRef) {
        return Interlocked.Exchange(ref Unsafe.As<SpinLockValue, int>(ref lockRef), 0) == 1;
    }

    private readonly ref SpinLockValue _lockRef;

    public readonly bool IsNull => Unsafe.IsNullRef(ref _lockRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReferenceSpinLock(ref SpinLockValue lockRef) {
        _lockRef = ref lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) Release(ref _lockRef);
    }
}
