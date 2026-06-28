
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ember.Logging;
using Ember.Memory;
using static Ember.Utils.SmartBox;

#pragma warning disable CS0420

namespace Ember.Utils;

public unsafe interface ISmartBoxValue<TThis> : IDisposable where TThis : unmanaged, ISmartBoxValue<TThis> {
    public void CopyTo(TThis* dst);
}

internal unsafe static class SmartBox {
    internal enum SmartBoxLock : uint {
        None,
        Reading,
        Writing
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct SmartBoxState(ulong value) {
        [FieldOffset(0)] public ulong Value = value;
        [FieldOffset(0)] public volatile int ReaderWriterCount;
        [FieldOffset(0)] public volatile ushort WaitingWriterCount;
        [FieldOffset(2)] public volatile ushort ActiveReaderCount;
        [FieldOffset(4)] public volatile SmartBoxLock Lock;
    }
}

public unsafe readonly struct SmartBox<T> : IDisposable where T : unmanaged, ISmartBoxValue<T> {

    public static SmartBox<T> Allocate(in T value) {
        Data* ptr = MemoryUtils.AllocateUninitialized<Data>();
        *ptr = new() {
            ReferenceCount = 1,
            Value = value
        };
        return new(ptr);
    }

    internal struct Data {
        public volatile uint ReferenceCount;
        public SmartBoxState State;
        public volatile uint WaitingReaderCount;
        public T Value;
    }

    private readonly Data* _ptr;
    public bool IsNull => _ptr == null;

    internal SmartBox(Data* ptr) {
        _ptr = ptr;
    }

    public SmartBox<T> AquireReference() {
        Interlocked.Increment(ref _ptr->ReferenceCount);
        return this;
    }

    private SmartBox<T> CopyReference() {
        SmartBox<T> copy = Allocate(default);
        _ptr->Value.CopyTo(&copy._ptr->Value);
        return copy;
    }

    public SmartBoxWriteLock<T> AquireExclusiveWrite(out SmartBox<T> newReference, bool disposeReference) {
        if (_ptr->ReferenceCount == 1) {
            SmartBoxWriteLock<T> writeLock = AquireWrite();

            if (_ptr->ReferenceCount != 1) {
                newReference = CopyReference();
                writeLock.Dispose();
                if (disposeReference) Dispose();
                return newReference.AquireWrite();
            }

            newReference = this;
            return writeLock;
        } else {
            using (SmartBoxReadLock<T> readLock = AquireRead())
                newReference = CopyReference();

            if (disposeReference) Dispose();
            return newReference.AquireWrite();
        }
    }

    public SmartBoxWriteLock<T> AquireWrite() {
        if (UnsafeInterlocked.CompareExchange(
            &_ptr->State.Lock, SmartBoxLock.Writing, SmartBoxLock.None
        ) == SmartBoxLock.None)
            return new SmartBoxWriteLock<T>(_ptr);

        Interlocked.Increment(ref _ptr->State.ReaderWriterCount);

        while (UnsafeInterlocked.CompareExchange(
            &_ptr->State.Lock, SmartBoxLock.Writing, SmartBoxLock.None
        ) == SmartBoxLock.None) {
            Thread.SpinWait(1);
        }

        Interlocked.Decrement(ref _ptr->State.ReaderWriterCount);

        return new(_ptr);
    }

    public SmartBoxReadLock<T> AquireRead() {
        bool firstIteration = true;

        while (true) {
            SmartBoxState oldState, newState;

            ushort activeReaderCount = _ptr->State.ActiveReaderCount;

            if (_ptr->State.Lock == SmartBoxLock.Reading && activeReaderCount != 0) {
                oldState = new() {
                    ActiveReaderCount = activeReaderCount,
                    WaitingWriterCount = 0,
                    Lock = SmartBoxLock.Reading
                };

                newState = new() {
                    ActiveReaderCount = (ushort)(activeReaderCount + 1),
                    WaitingWriterCount = 0,
                    Lock = SmartBoxLock.Reading
                };
            } else {
                oldState = new() {
                    ActiveReaderCount = 0,
                    WaitingWriterCount = 0,
                    Lock = SmartBoxLock.None
                };

                newState = new() {
                    ActiveReaderCount = 1,
                    WaitingWriterCount = 0,
                    Lock = SmartBoxLock.Reading
                };
            }

            if (Interlocked.CompareExchange(
                ref _ptr->State.Value,
                newState.Value, oldState.Value
            ) == oldState.Value) {
                if (!firstIteration) Interlocked.Decrement(ref _ptr->WaitingReaderCount);

                return new(_ptr);
            }

            if (firstIteration) {
                Interlocked.Increment(ref _ptr->WaitingReaderCount);
                firstIteration = false;
            }

            Thread.SpinWait(1);
        }
    }

    public void Dispose() {
        if (Interlocked.Decrement(ref _ptr->ReferenceCount) == 0) {
            AquireWrite();
            _ptr->Value.Dispose();
            MemoryUtils.Free(_ptr);
        }
    }
}

public unsafe readonly struct SmartBoxReadLock<T> : IDisposable where T : unmanaged, ISmartBoxValue<T> {

    private readonly SmartBox<T>.Data* _ptr;
    public bool IsNull => _ptr == null;

    internal SmartBoxReadLock(SmartBox<T>.Data* ptr) {
        _ptr = ptr;
    }

    public T* GetData() {
        if (IsNull) return null;
        return &_ptr->Value;
    }

    public void Dispose() {
        if (IsNull) return;
        Debug.Assert(_ptr->State.Lock == SmartBoxLock.Reading, _ptr->State.ActiveReaderCount + " " + _ptr->State.Lock);
        Debug.Assert(_ptr->State.ActiveReaderCount != 0);

        if (Interlocked.Add(ref _ptr->State.ReaderWriterCount, -(1 << 16)) >> 16 == 0) {
            _ptr->State.Lock = SmartBoxLock.None;
        }
    }
}

public unsafe readonly struct SmartBoxWriteLock<T> : IDisposable where T : unmanaged, ISmartBoxValue<T> {

    private readonly SmartBox<T>.Data* _ptr;
    public bool IsNull => _ptr == null;

    internal SmartBoxWriteLock(SmartBox<T>.Data* ptr) {
        _ptr = ptr;
    }

    public T* GetData() {
        if (IsNull) return null;
        return &_ptr->Value;
    }

    public void Dispose() {
        if (IsNull) return;
        Debug.Assert(_ptr->State.Lock == SmartBoxLock.Writing);
        Debug.Assert(_ptr->State.ActiveReaderCount == 0);

        _ptr->State.Lock = SmartBoxLock.None;
    }
}