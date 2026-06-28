
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Memory;

public unsafe readonly struct PinnedMemoryHandle : IDisposable {

    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Buffers/MemoryHandle.cs
    private struct ExposedMemoryHandle {
        public void* Pointer;
        public GCHandle Handle;
        public IPinnable? Pinnable;
    }

    public static PinnedMemoryHandle FromMemoryHandle(MemoryHandle memoryHandle) {
        return FromMemoryHandle(ref memoryHandle);
    }

    public static PinnedMemoryHandle FromMemoryHandle(ref MemoryHandle memoryHandle) {
        ref ExposedMemoryHandle exposedMemoryHandle = ref Unsafe.As<MemoryHandle, ExposedMemoryHandle>(ref memoryHandle);

        bool hasHandle = exposedMemoryHandle.Handle.IsAllocated;
        bool hasPinnable = exposedMemoryHandle.Pinnable != null;

        GCHandle handle;

        if (hasHandle && !hasPinnable) {
            if (exposedMemoryHandle.Handle.Target is not IPinnable) {
                // If we only have a handle, we just store that handle
                handle = exposedMemoryHandle.Handle;
            } else {
                // Later, we check if the handle is an instance to IPinnable to see if we need to unpin it.
                //  That's a problem in the case the the Handle just happens to be an IPinnable. In this case
                //  we wrap our handle in a type that is not IPinnable
                handle = GCHandle.Alloc(new DoubleMemoryHandle(exposedMemoryHandle.Handle, null));
            }
        } else if (hasPinnable && !hasHandle) {
            // If we only have a pinnable, we store a handle to that pinnable
            handle = GCHandle.Alloc(exposedMemoryHandle.Pinnable);
        } else if (hasHandle && hasPinnable) {
            // If we have both, store a handle to an object containing the handle and pinnable
            handle = GCHandle.Alloc(new DoubleMemoryHandle(exposedMemoryHandle.Handle, exposedMemoryHandle.Pinnable));
        } else {
            // If we have neither, we don't need to store anything
            handle = default;
        }

        return new PinnedMemoryHandle(handle);
    }

    private record DoubleMemoryHandle(GCHandle Handle, IPinnable? Pinnable);

    public readonly GCHandle Handle;

    private PinnedMemoryHandle(GCHandle handle) {
        Handle = handle;
    }

    public void Dispose() {
        if (!Handle.IsAllocated) return;

        object? target = Handle.Target;

        if (target is IPinnable pinnable) {
            pinnable.Unpin();
        } else if (target is DoubleMemoryHandle doubleMemoryHandle) {
            doubleMemoryHandle.Handle.Free();
            doubleMemoryHandle.Pinnable?.Unpin();
        }

        Handle.Free();
    }
}