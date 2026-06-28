
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
}