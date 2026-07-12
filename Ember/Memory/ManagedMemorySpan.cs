
using System;
using System.Buffers;

namespace Ember.Memory;

public readonly unsafe struct ManagedMemorySpan<T> : IDisposable where T : unmanaged {

    public static ManagedMemorySpan<T> PinMemory(Memory<T> memory) {
        MemoryHandle memoryHandle = memory.Pin();
        return new(PinnedMemoryHandle.FromMemoryHandle(memoryHandle), new((T*)memoryHandle.Pointer, memory.Length));
    }

    public readonly PinnedMemoryHandle MemoryHandle;
    public readonly MemorySpan<T> MemorySpan;

    public nint LengthLong => MemorySpan.LengthLong;
    public int Length => MemorySpan.Length;
    public T* Pointer => MemorySpan.Pointer;
    public bool IsNull => MemorySpan.IsNull;

    public ManagedMemorySpan(PinnedMemoryHandle memoryHandle, MemorySpan<T> memorySpan) {
        MemoryHandle = memoryHandle;
        MemorySpan = memorySpan;
    }

    public void Dispose() {
        MemoryHandle.Dispose();
    }
}

public static class ManagedMemorySpanyExts {

    public static ManagedMemorySpan<T> PinSpan<T>(this Memory<T> memory) where T : unmanaged {
        return ManagedMemorySpan<T>.PinMemory(memory);
    }

    public static ManagedMemorySpan<T> PinSpan<T>(this T[] array) where T : unmanaged {
        return ManagedMemorySpan<T>.PinMemory(array);
    }
}