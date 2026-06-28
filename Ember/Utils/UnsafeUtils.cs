
using System.Runtime.CompilerServices;

namespace Ember.Utils;

public static unsafe class UnsafeUtils {

    // See https://stackoverflow.com/questions/78376066/how-to-reinterpret-a-ref-t-as-a-ref-nint
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nint AsNIntRef<T>(ref T* reference) where T : unmanaged {
        fixed (void* pRef = &reference) return ref Unsafe.AsRef<nint>(pRef);
    }

}