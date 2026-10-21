
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ember.Memory;

namespace Ember.Utils;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct SlimReaderWriterLock {

    private enum WriteLockState : int {
        None = 0,
        UpgradeableReading = 1,
        WriteWaitingForReaders = 2,
        Writing = 3
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HeldRefSlimReaderSpinLock AcquireRead(ref SlimReaderWriterLock lockRef) {
        SlimSpinLock.SpinUntilNotEqual(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders);

        Interlocked.Increment(ref lockRef._readerCount);
        WriteLockState writeState = UnsafeVolatile.Read(ref lockRef._writeState);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void SlowPath(ref SlimReaderWriterLock lockRef, WriteLockState writeState) {
            while (writeState == WriteLockState.WriteWaitingForReaders) {
                Interlocked.Decrement(ref lockRef._readerCount);
                SlimSpinLock.SpinUntilNotEqual(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders);
                Interlocked.Increment(ref lockRef._readerCount);
                writeState = UnsafeVolatile.Read(ref lockRef._writeState);
            }

            if (writeState == WriteLockState.Writing)
                SlimSpinLock.SpinUntilNotEqual(ref lockRef._writeState, WriteLockState.Writing);
        }

        if (writeState > WriteLockState.UpgradeableReading)
            SlowPath(ref lockRef, writeState);

        return new(ref lockRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HeldPtrSlimReaderSpinLock AcquireRead(SlimReaderWriterLock* lockPtr) {
        SlimSpinLock.SpinUntilNotEqual(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders);

        Interlocked.Increment(ref lockPtr->_readerCount);
        WriteLockState writeState = UnsafeVolatile.Read(ref lockPtr->_writeState);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void SlowPath(SlimReaderWriterLock* lockPtr, WriteLockState writeState) {
            while (writeState == WriteLockState.WriteWaitingForReaders) {
                Interlocked.Decrement(ref lockPtr->_readerCount);
                SlimSpinLock.SpinUntilNotEqual(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders);
                Interlocked.Increment(ref lockPtr->_readerCount);
                writeState = UnsafeVolatile.Read(ref lockPtr->_writeState);
            }

            if (writeState == WriteLockState.Writing)
                SlimSpinLock.SpinUntilNotEqual(ref lockPtr->_writeState, WriteLockState.Writing);
        }

        if (writeState > WriteLockState.UpgradeableReading)
            SlowPath(lockPtr, writeState);

        return new(lockPtr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireRead(scoped ref SlimReaderWriterLock lockRef)
        => TryAcquireRead(ref lockRef, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireRead(ref SlimReaderWriterLock lockRef, out HeldRefSlimReaderSpinLock refReadLock) {
        Interlocked.Increment(ref lockRef._readerCount);
        WriteLockState writeState = UnsafeVolatile.Read(ref lockRef._writeState);

        if (writeState > WriteLockState.UpgradeableReading) {
            Interlocked.Decrement(ref lockRef._readerCount);
            goto fail;
        }

        refReadLock = new(ref lockRef);
        return true;
fail:
        refReadLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireRead(SlimReaderWriterLock* lockPtr)
        => TryAcquireRead(lockPtr, out _);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireRead(SlimReaderWriterLock* lockPtr, out HeldPtrSlimReaderSpinLock refReadLock) {
        Interlocked.Increment(ref lockPtr->_readerCount);
        WriteLockState writeState = UnsafeVolatile.Read(ref lockPtr->_writeState);

        if (writeState > WriteLockState.UpgradeableReading) {
            Interlocked.Decrement(ref lockPtr->_readerCount);
            goto fail;
        }

        refReadLock = new(lockPtr);
        return true;
fail:
        refReadLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseRead(scoped ref SlimReaderWriterLock lockRef) {
        int readerCount = Interlocked.Decrement(ref lockRef._readerCount);
#if EMBER_SAFETY_CHECKS
        if (readerCount < 0)
            throw new InvalidOperationException("Read lock was released more times than it has been acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseRead(SlimReaderWriterLock* lockPtr) {
        int readerCount = Interlocked.Decrement(ref lockPtr->_readerCount);
#if EMBER_SAFETY_CHECKS
        if (readerCount < 0)
            throw new InvalidOperationException("Read lock was released more times than it has been acquired.");
#endif
    }

    public static HeldRefSlimWriterSpinLock AcquireWrite(ref SlimReaderWriterLock lockRef) {
        SlimSpinLock.SpinUntilCompareExchange(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders, WriteLockState.None);
        SlimSpinLock.SpinUntilEqual(ref lockRef._readerCount, 0);
        UnsafeVolatile.Write(ref lockRef._writeState, WriteLockState.Writing);
        return new(ref lockRef);
    }

    public static HeldPtrSlimWriterSpinLock AcquireWrite(SlimReaderWriterLock* lockPtr) {
        SlimSpinLock.SpinUntilCompareExchange(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders, WriteLockState.None);
        SlimSpinLock.SpinUntilEqual(ref lockPtr->_readerCount, 0);
        UnsafeVolatile.Write(ref lockPtr->_writeState, WriteLockState.Writing);
        return new(lockPtr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireWrite(scoped ref SlimReaderWriterLock lockRef, bool waitForReaders = true)
        => TryAcquireWrite(ref lockRef, out _, waitForReaders);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireWrite(ref SlimReaderWriterLock lockRef, out HeldRefSlimWriterSpinLock refWriteLock, bool waitForReaders = true) {
        if (!UnsafeInterlocked.TryCompareExchange(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders, WriteLockState.None))
            goto fail;

        if (waitForReaders) {
            SlimSpinLock.SpinUntilEqual(ref lockRef._readerCount, 0);
        } else {
            if (UnsafeVolatile.Read(ref lockRef._readerCount) != 0) {
                UnsafeVolatile.Write(ref lockRef._writeState, WriteLockState.None);
                goto fail;
            }
        }

        UnsafeVolatile.Write(ref lockRef._writeState, WriteLockState.Writing);

        refWriteLock = new(ref lockRef);
        return true;
fail:
        refWriteLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireWrite(SlimReaderWriterLock* lockPtr, bool waitForReaders = true)
        => TryAcquireWrite(lockPtr, out _, waitForReaders);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAcquireWrite(SlimReaderWriterLock* lockPtr, out HeldPtrSlimWriterSpinLock ptrWriteLock, bool waitForReaders = true) {
        if (!UnsafeInterlocked.TryCompareExchange(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders, WriteLockState.None))
            goto fail;

        if (waitForReaders) {
            SlimSpinLock.SpinUntilEqual(ref lockPtr->_readerCount, 0);
        } else {
            if (UnsafeVolatile.Read(ref lockPtr->_readerCount) != 0) {
                UnsafeVolatile.Write(ref lockPtr->_writeState, WriteLockState.None);
                goto fail;
            }
        }

        UnsafeVolatile.Write(ref lockPtr->_writeState, WriteLockState.Writing);

        ptrWriteLock = new(lockPtr);
        return true;
fail:
        ptrWriteLock = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseWrite(scoped ref SlimReaderWriterLock lockRef) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockRef._writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock was not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseWrite(SlimReaderWriterLock* lockPtr) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockPtr->_writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilNotWriting(scoped ref SlimReaderWriterLock lockRef) {

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Slow(ref SlimReaderWriterLock lockRef, WriteLockState state) {
            if (state == WriteLockState.WriteWaitingForReaders)
                state = SlimSpinLock.SpinUntilNotEqual(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders);

            if (state == WriteLockState.Writing)
                SlimSpinLock.SpinUntilNotEqual(ref lockRef._writeState, WriteLockState.Writing);
        }

        WriteLockState state = UnsafeVolatile.Read(ref lockRef._writeState);

        if (state > WriteLockState.UpgradeableReading)
            Slow(ref lockRef, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpinUntilNotWriting(SlimReaderWriterLock* lockPtr) {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Slow(SlimReaderWriterLock* lockPtr, WriteLockState state) {
        if (state == WriteLockState.WriteWaitingForReaders)
            state = SlimSpinLock.SpinUntilNotEqual(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders);

        if (state == WriteLockState.Writing)
            SlimSpinLock.SpinUntilNotEqual(ref lockPtr->_writeState, WriteLockState.Writing);
        }

        WriteLockState state = UnsafeVolatile.Read(ref lockPtr->_writeState);

        if (state > WriteLockState.UpgradeableReading)
            Slow(lockPtr, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AcquireUpgradeableRead(ref SlimReaderWriterLock lockRef) {
        SlimSpinLock.SpinUntilCompareExchange(ref lockRef._writeState, WriteLockState.UpgradeableReading, WriteLockState.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AcquireUpgradeableRead(SlimReaderWriterLock* lockPtr) {
        SlimSpinLock.SpinUntilCompareExchange(ref lockPtr->_writeState, WriteLockState.UpgradeableReading, WriteLockState.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseUpgradeableRead(ref SlimReaderWriterLock lockRef) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockRef._writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.UpgradeableReading)
            throw new InvalidOperationException("Upgradeable read lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseUpgradeableRead(SlimReaderWriterLock* lockPtr) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockPtr->_writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.UpgradeableReading)
            throw new InvalidOperationException("Upgradeable read lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpgradeRead(ref SlimReaderWriterLock lockRef) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockRef._writeState, WriteLockState.WriteWaitingForReaders);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.UpgradeableReading)
            throw new InvalidOperationException("Upgradeable read lock is not acquired.");
#endif
        SlimSpinLock.SpinUntilEqual(ref lockRef._readerCount, 0);
        UnsafeVolatile.Write(ref lockRef._writeState, WriteLockState.Writing);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpgradeRead(SlimReaderWriterLock* lockPtr) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockPtr->_writeState, WriteLockState.WriteWaitingForReaders);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.UpgradeableReading)
            throw new InvalidOperationException("Upgradeable read lock is not acquired.");
#endif
        SlimSpinLock.SpinUntilEqual(ref lockPtr->_readerCount, 0);
        UnsafeVolatile.Write(ref lockPtr->_writeState, WriteLockState.Writing);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DowngradeWriteToRead(ref SlimReaderWriterLock lockRef) {
        Interlocked.Increment(ref lockRef._readerCount);

        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockRef._writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DowngradeWriteToRead(SlimReaderWriterLock* lockPtr) {
        Interlocked.Increment(ref lockPtr->_readerCount);

        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockPtr->_writeState, WriteLockState.None);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DowngradeWriteToUpgradeableRead(ref SlimReaderWriterLock lockRef) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockRef._writeState, WriteLockState.UpgradeableReading);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock is not acquired.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DowngradeWriteToUpgradeableRead(SlimReaderWriterLock* lockPtr) {
        WriteLockState oldState = UnsafeInterlocked.Exchange(ref lockPtr->_writeState, WriteLockState.UpgradeableReading);
#if EMBER_SAFETY_CHECKS
        if (oldState != WriteLockState.Writing)
            throw new InvalidOperationException("Write lock is not acquired.");
#endif
    }

    [FieldOffset(0)] private WriteLockState _writeState;
    [FieldOffset(4)] private int _readerCount;
    
    // Do not remove this field. Without it, the struct will be 4 byte aligned, which allows it to be placed
    //   on a cache line boundry. That means atomic operations will cause a full bus lock, degrading peformance.
    [FieldOffset(0)] private long _combined;

    public void Init() {
        this = default;
    }

    public readonly override string ToString() {
        // Make sure we read both values at once to get an accurate snapshot
        SlimReaderWriterLock state = UnsafeInterlocked.Read(in this);
        return $"[Read = {state._readerCount}, Write = {state._writeState}]";
    }
}

public readonly ref struct HeldRefSlimReaderSpinLock {
    private readonly ref SlimReaderWriterLock _lock;
    public readonly bool IsNull => Unsafe.IsNullRef(ref _lock);
    public ref SlimReaderWriterLock Lock => ref _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldRefSlimReaderSpinLock(ref SlimReaderWriterLock lockRef) {
        _lock = ref lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) SlimReaderWriterLock.ReleaseRead(ref _lock);
    }
}

public readonly ref struct HeldRefSlimWriterSpinLock {
    private readonly ref SlimReaderWriterLock _lock;
    public readonly bool IsNull => Unsafe.IsNullRef(ref _lock);
    public ref SlimReaderWriterLock Lock => ref _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldRefSlimWriterSpinLock(ref SlimReaderWriterLock lockRef) {
        _lock = ref lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) SlimReaderWriterLock.ReleaseWrite(ref _lock);
    }
}

public readonly unsafe struct HeldPtrSlimReaderSpinLock {
    private readonly SlimReaderWriterLock* _lock;
    public readonly bool IsNull => _lock == null;
    public SlimReaderWriterLock* Lock => _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldPtrSlimReaderSpinLock(SlimReaderWriterLock* lockRef) {
        _lock = lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) SlimReaderWriterLock.ReleaseRead(_lock);
    }
}

public readonly unsafe struct HeldPtrSlimWriterSpinLock {
    private readonly SlimReaderWriterLock* _lock;
    public readonly bool IsNull => _lock == null;
    public SlimReaderWriterLock* Lock => _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HeldPtrSlimWriterSpinLock(SlimReaderWriterLock* lockRef) {
        _lock = lockRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        if (!IsNull) SlimReaderWriterLock.ReleaseWrite(_lock);
    }
}