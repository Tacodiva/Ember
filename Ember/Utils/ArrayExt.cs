
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Utils;

public static class ArrayExt {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T UnsafeGetReference<T>(this T[] array, int index) {
#if EMBER_SAFETY_CHECKS
        return ref array[index];
#else
        return ref Unsafe.Add(ref UnsafeGetReference(array), index);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T UnsafeGetReference<T>(this T[] array) {
        return ref MemoryMarshal.GetArrayDataReference(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetReferenceIndex<TElem, TRef>(this TElem[] array, ref TRef interiorReference) {
#if EMBER_SAFETY_CHECKS
        ArgumentNullException.ThrowIfNull(array, nameof(array));
#endif

        ref byte arrayStart = ref Unsafe.As<TElem, byte>(ref MemoryMarshal.GetArrayDataReference(array!));
        ref byte refByte = ref Unsafe.As<TRef, byte>(ref interiorReference);

        nuint refByteOffset = (nuint)Unsafe.ByteOffset(ref arrayStart, ref refByte);
        nuint arrayByteLength = (nuint)Unsafe.SizeOf<TElem>() * (nuint)array.Length;

        if (refByteOffset >= arrayByteLength) return -1;

        nuint index = refByteOffset / (nuint)Unsafe.SizeOf<TElem>();

        return (int)index;
    }
}