
#if DEBUG
#define EMBER_TRACK_ALLOCATIONS
#define EMBER_VALIDATE_ALLOCATIONS
#endif

#if EMBER_VALIDATE_ALLOCATIONS && !EMBER_TRACK_ALLOCATIONS
#error Cannot validate allocations if allocations are not tracked
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using Ember.Logging;
using Ember.Utils;

namespace Ember.Memory;

public static unsafe class MemoryUtils {

#if EMBER_TRACK_ALLOCATIONS
    private readonly record struct AllocationInfo(nuint Length, Type? Type, StackTrace? Trace) {
        public AllocationInfo(nuint length, Type? type) : this(length, type, TrackAllocationTraces ? new(2, TrackAllocationTracesDebug) : null) { }
    }

    private static readonly ConcurrentDictionary<nint, AllocationInfo> _AllocationMap = [];
    private static nuint _allocatedBytes;
    private static readonly Logger _Logger = Log.CreateLogger("AllocTracker");
#endif

    public static bool IsTrackingAllocations {
        get {
#if EMBER_TRACK_ALLOCATIONS
            return true;
#else
            return false;
#endif
        }
    }

    public static bool TrackAllocationTraces { get; set; }
    public static bool TrackAllocationTracesDebug { get; set; }

    public static int AllocationCount {
        get {
#if EMBER_TRACK_ALLOCATIONS
            return _AllocationMap.Count;
#else
            return -1;
#endif
        }
    }

    public static nuint AllocatedBytes {
        get {
#if EMBER_TRACK_ALLOCATIONS
            return _allocatedBytes;
#else
            return 0;
#endif
        }
    }

    public static bool AssertNoAllocations() {
#if EMBER_TRACK_ALLOCATIONS
        for (int i = 0; i < 8; i++) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        if (AllocationCount > 0) {
            _Logger.Error($"{AllocationCount} allocations not cleaned up.");
            _Logger.Error(DumpAllocations());
            return false;
        } else {
            _Logger.Fine("All allocations cleaned.");
        }
#endif
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("EMBER_VALIDATE_ALLOCATIONS")]
    public static void ValidateAllocation<T>(T* ptr, string? ptrName = null, int expectedLength = 1) where T : unmanaged {
        ValidateAllocation((nint)ptr, ptrName, sizeof(T) * expectedLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("EMBER_VALIDATE_ALLOCATIONS")]
    public static void ValidateAllocation(nint ptr, string? ptrName, int expectedLength) {
#if EMBER_TRACK_ALLOCATIONS
        ArgumentOutOfRangeException.ThrowIfNegative(expectedLength, nameof(expectedLength));
        if (ptr == 0)
            throw new NullReferenceException($"Allocation validation caught null pointer{(ptrName == null ? "" : $" '{ptrName}'")}. Expected allocation of length 0x{expectedLength:X}.");
        if (!_AllocationMap.TryGetValue(ptr, out AllocationInfo info))
            throw new AccessViolationException($"Allocation validation caught invalid pointer 0x{ptr:X}{(ptrName == null ? "" : $" '{ptrName}'")}. Expected allocation of length 0x{expectedLength:X}.");
        if (info.Length != (nuint)expectedLength)
            throw new AccessViolationException($"Allocation validation caught wrongly sized allocation at 0x{ptr:X}{(ptrName == null ? "" : $" '{ptrName}'")}. Expected 0x{expectedLength:X} bytes but found 0x{info.Length:X}.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("EMBER_TRACK_ALLOCATIONS")]
    private static void TrackAllocation(void* allocation, nuint length, Type? type) {
#if EMBER_TRACK_ALLOCATIONS
        _AllocationMap.TryAdd((nint)allocation, new(length, type));
        UnsafeInterlocked.Add(ref _allocatedBytes, length);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("EMBER_TRACK_ALLOCATIONS")]
    private static void TrackFree(void* ptr, ref Type? type) {
#if EMBER_TRACK_ALLOCATIONS
        if (_AllocationMap.TryRemove((nint)ptr, out AllocationInfo allocationRecord)) {
            UnsafeInterlocked.Add(ref _allocatedBytes, (nuint)(-(nint)allocationRecord.Length));
            if (!Unsafe.IsNullRef(ref type)) type = allocationRecord.Type;
        } else {
            _Logger.WarnTrace($"Freeing an unknown allocation at 0x{(nint)ptr:X}");
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint CheckAllocationLength(int length) {
#if EMBER_SAFETY_CHECKS
        ArgumentOutOfRangeException.ThrowIfNegative(length);
#endif
        return (nuint)length;
    }

    #region Base Functions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocateUninitialized(nuint length, Type? type) {
        void* allocation = NativeMemory.Alloc(length);
        TrackAllocation(allocation, length, type);
        return allocation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocateAlignedUninitialized(nuint length, nuint alignment, Type? type) {
        void* allocation = NativeMemory.AlignedAlloc(length, alignment);
        TrackAllocation(allocation, length, type);
        return allocation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocateInitialized(nuint length, Type? type, byte value = 0) {
        void* allocation = AllocateUninitialized(length, type);
        Fill(allocation, length, value);
        return allocation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocateAlignedInitialized(nuint length, nuint alignment, Type? type, byte value = 0) {
        void* allocation = AllocateAlignedUninitialized(length, alignment, type);
        Fill(allocation, length, value);
        return allocation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* ResizeUninitialized(void* ptr, nuint newLength, Type? fallbackType) {
        if (ptr == null)
            return AllocateUninitialized(newLength, fallbackType);

        TrackFree(ptr, ref fallbackType);
        void* newPtr = NativeMemory.Realloc(ptr, newLength);
        TrackAllocation(newPtr, newLength, fallbackType);
        return newPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* ResizeInitialized(void* ptr, nuint oldLength, nuint newLength, Type? fallbackType, byte value = 0) {
        void* newPtr = ResizeUninitialized(ptr, newLength, fallbackType);
        Fill((void*)((nuint)ptr + oldLength), newLength - oldLength, value);
        return newPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* ResizeAlignedUninitialized(void* ptr, nuint newLength, nuint alignment, Type? fallbackType) {
        if (ptr == null)
            return AllocateAlignedUninitialized(newLength, alignment, fallbackType);

        TrackFree(ptr, ref fallbackType);
        void* newPtr = NativeMemory.AlignedRealloc(ptr, newLength, alignment);
        TrackAllocation(newPtr, newLength, fallbackType);
        return newPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* ResizeAlignedInitialized(void* ptr, nuint oldLength, nuint newLength, nuint alignment, Type? fallbackType, byte value = 0) {
        void* newPtr = ResizeAlignedUninitialized(ptr, newLength, alignment, fallbackType);
        Fill((void*)((nuint)ptr + oldLength), newLength - oldLength, value);
        return newPtr;
    }
    #endregion

    #region Untyped
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateUninitialized(nuint length) {
        return AllocateUninitialized(length, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Allocate(nuint length, byte initValue = 0) {
        return AllocateInitialized(length, null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateUninitialized(int length) {
        return AllocateUninitialized(CheckAllocationLength(length), null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Allocate(int length, byte initValue = 0) {
        return AllocateInitialized(CheckAllocationLength(length), null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateAlignedUninitialized(nuint length, nuint alignment) {
        return AllocateAlignedUninitialized(length, alignment, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateAligned(nuint length, nuint alignment, byte initValue = 0) {
        return AllocateAlignedInitialized(length, alignment, null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateAlignedUninitialized(int length, int alignment) {
        return AllocateAlignedUninitialized(CheckAllocationLength(length), CheckAllocationLength(alignment), null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocateAligned(int length, int alignment, byte initValue = 0) {
        return AllocateAlignedInitialized(CheckAllocationLength(length), CheckAllocationLength(alignment), null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Resize(void* ptr, nuint oldLength, nuint newLength, byte initValue = 0) {
        return ResizeInitialized(ptr, oldLength, newLength, null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Resize(void* ptr, int oldLength, int newLength, byte initValue = 0) {
        return Resize(ptr, CheckAllocationLength(oldLength), CheckAllocationLength(newLength), initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeUninitialized(void* ptr, nuint newLength) {
        return ResizeUninitialized(ptr, newLength, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeUninitialized(void* ptr, int newLength) {
        return ResizeUninitialized(ptr, CheckAllocationLength(newLength));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeAligned(void* ptr, nuint oldLength, nuint newLength, nuint alignment, byte initValue = 0) {
        return ResizeAlignedInitialized(ptr, oldLength, newLength, alignment, null, initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeAligned(void* ptr, int oldLength, int newLength, int alignment, byte initValue = 0) {
        return ResizeAligned(ptr, CheckAllocationLength(oldLength), CheckAllocationLength(newLength), CheckAllocationLength(alignment), initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeAlignedUninitialized(void* ptr, nuint newLength, nuint alignment) {
        return ResizeAlignedUninitialized(ptr, newLength, alignment, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ResizeAlignedUninitialized(void* ptr, int newLength, int alignment) {
        return ResizeAlignedUninitialized(ptr, CheckAllocationLength(newLength), CheckAllocationLength(alignment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(void* ptr) {
        if (ptr == null) return;
        NativeMemory.Free(ptr);
        TrackFree(ptr, ref Unsafe.NullRef<Type?>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(nint ptr) {
        Free((void*)ptr);
    }
    #endregion

    #region MemorySpan
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemorySpan AllocateMemorySpanUninitialized(int length) {
        return new(AllocateUninitialized(CheckAllocationLength(length), typeof(MemorySpan)), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemorySpan AllocateMemorySpan(int length, byte initValue = 0) {
        return new(AllocateInitialized(CheckAllocationLength(length), typeof(MemorySpan), initValue), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(MemorySpan span) {
        Free(span.Pointer);
    }
    #endregion

    #region MemorySpan<T>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemorySpan<T> AllocateMemorySpanUninitialized<T>(int length = 1) where T : unmanaged {
        return new MemorySpan<T>((T*)AllocateUninitialized(CheckAllocationLength(length) * (nuint)sizeof(T), typeof(MemorySpan<T>)), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemorySpan<T> AllocateMemorySpan<T>(int length, byte initValue = 0) where T : unmanaged {
        return new MemorySpan<T>((T*)AllocateInitialized(CheckAllocationLength(length) * (nuint)sizeof(T), typeof(MemorySpan<T>), initValue), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(MemorySpan<T> span) where T : unmanaged {
        Free(span.Pointer);
    }
    #endregion

    #region T
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* AllocateUninitialized<T>(int length = 1) where T : unmanaged {
        return (T*)AllocateUninitialized(CheckAllocationLength(length) * (nuint)sizeof(T), typeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Allocate<T>(int length = 1, byte initValue = 0) where T : unmanaged {
        return (T*)AllocateInitialized(CheckAllocationLength(length) * (nuint)sizeof(T), typeof(T), initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Resize<T>(T* ptr, nuint oldLength, nuint newLength, byte initValue = 0) where T : unmanaged {
        return (T*)ResizeInitialized(ptr, oldLength * (nuint)sizeof(T), newLength * (nuint)sizeof(T), typeof(T), initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Resize<T>(T* ptr, int oldLength, int newLength, byte initValue = 0) where T : unmanaged {
        return Resize<T>(ptr, CheckAllocationLength(oldLength), CheckAllocationLength(newLength), initValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ResizeUninitialized<T>(T* ptr, nuint newLength) where T : unmanaged {
        return (T*)ResizeUninitialized(ptr, newLength * (nuint)sizeof(T), typeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ResizeUninitialized<T>(T* ptr, int newLength) where T : unmanaged {
        return ResizeUninitialized<T>(ptr, CheckAllocationLength(newLength));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(T* ptr) where T : unmanaged {
        Free((void*)ptr);
    }
    #endregion

    #region Copy
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* src, void* dst, nuint amount) {
        NativeMemory.Copy(src, dst, amount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* src, void* dst, int amount) {
        Copy(src, dst, CheckAllocationLength(amount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(T* src, T* dst, nuint amount) where T : unmanaged {
        Copy((void*)src, dst, amount * (nuint)sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(T* src, T* dst, int amount) where T : unmanaged {
        Copy<T>(src, dst, CheckAllocationLength(amount));
    }
    #endregion

    #region Fill
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(void* ptr, nuint length, byte value = 0) {
        NativeMemory.Fill(ptr, length, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(void* ptr, int length, byte value = 0) {
        Fill(ptr, CheckAllocationLength(length), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(T* ptr, int length = 1, byte value = 0) where T : unmanaged {
        Fill((void*)ptr, CheckAllocationLength(length) * (nuint)sizeof(T), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(T* ptr, nuint length = 1, byte value = 0) where T : unmanaged {
        Fill((void*)ptr, length * (nuint)sizeof(T), value);
    }
    #endregion

    #region Compare
    public static bool Compare(void* aIn, void* bIn, nuint length) {

        const int blockSizeLog2 = 7;
        const int blockSize = 1 << blockSizeLog2; // 128
        const nuint blockMask = blockSize - 1; // 0b01111111

        byte* a = (byte*)aIn;
        byte* b = (byte*)bIn;

        nuint blockRemainder = length & blockMask;

        byte* blockEnd = (byte*)(((nuint)a + length) & ~blockMask);

        while (a < blockEnd) {

            if (Vector512.IsHardwareAccelerated) {
                Vector512<ulong>* a1 = (Vector512<ulong>*)a;
                Vector512<ulong>* b1 = (Vector512<ulong>*)b;

                if (!Vector512.EqualsAll(*(a1), *(b1)) || !Vector512.EqualsAll(*(a1 + 1), *(b1 + 1)))
                    return false;
            } else if (Vector256.IsHardwareAccelerated) {
                Vector256<ulong>* a1 = (Vector256<ulong>*)a;
                Vector256<ulong>* b1 = (Vector256<ulong>*)b;

                if (!Vector256.EqualsAll(*(a1), *(b1)) || !Vector256.EqualsAll(*(a1 + 1), *(b1 + 1)) ||
                    !Vector256.EqualsAll(*(a1 + 2), *(b1 + 2)) || !Vector256.EqualsAll(*(a1 + 3), *(b1 + 3)))
                    return false;
            } else if (Vector128.IsHardwareAccelerated) {
                Vector128<ulong>* a1 = (Vector128<ulong>*)a;
                Vector128<ulong>* b1 = (Vector128<ulong>*)b;

                if (!Vector128.EqualsAll(*(a1), *(b1)) || !Vector128.EqualsAll(*(a1 + 1), *(b1 + 1)) ||
                    !Vector128.EqualsAll(*(a1 + 2), *(b1 + 2)) || !Vector128.EqualsAll(*(a1 + 3), *(b1 + 3)) ||
                    !Vector128.EqualsAll(*(a1 + 4), *(b1 + 4)) || !Vector128.EqualsAll(*(a1 + 5), *(b1 + 5)) ||
                    !Vector128.EqualsAll(*(a1 + 6), *(b1 + 6)) || !Vector128.EqualsAll(*(a1 + 7), *(b1 + 7)))
                    return false;
            } else {
                ulong* a1 = (ulong*)a;
                ulong* b1 = (ulong*)b;

                if (*(a1) != *(b1) || *(a1 + 1) != *(b1 + 1) ||
                    *(a1 + 2) != *(b1 + 2) || *(a1 + 3) != *(b1 + 3) ||
                    *(a1 + 4) != *(b1 + 4) || *(a1 + 5) != *(b1 + 5) ||
                    *(a1 + 6) != *(b1 + 6) || *(a1 + 7) != *(b1 + 7) ||
                    *(a1 + 8) != *(b1 + 8) || *(a1 + 9) != *(b1 + 9) ||
                    *(a1 + 10) != *(b1 + 10) || *(a1 + 11) != *(b1 + 11) ||
                    *(a1 + 12) != *(b1 + 12) || *(a1 + 13) != *(b1 + 13) ||
                    *(a1 + 14) != *(b1 + 14) || *(a1 + 15) != *(b1 + 15))
                    return false;
            }

            a += blockSize;
            b += blockSize;
        }

        for (nuint i = 0; i < blockRemainder; i++)
            if (a[i] != b[i]) return false;

        return true;
    }

    public static bool Compare(void* a, void* b, int length) {
        return Compare(a, b, CheckAllocationLength(length));
    }
    #endregion

    public static string DumpAllocations() {
#if EMBER_TRACK_ALLOCATIONS
        if (_AllocationMap.IsEmpty) return "[No allocations]";
        StringBuilder sb = new();
        foreach (KeyValuePair<nint, AllocationInfo> allocation in _AllocationMap) {
            sb.AppendLine($"0x{allocation.Key:X} - {allocation.Value.Type?.ToString() ?? "[Unknown Type]"} (0x{allocation.Value.Length:X} bytes)");
            StackTrace? trace = allocation.Value.Trace;
            if (trace == null) {
                if (TrackAllocationTraces)
                    sb.AppendLine("  [No trace information]");
            } else {
                sb.AppendLine("  " + trace.ToString().Replace("\n", "\n  "));
            }
        }
        return sb.ToString();
#else
        return "[Allocations not tracked]";
#endif
    }

}