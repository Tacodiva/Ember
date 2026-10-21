
using System.Runtime.CompilerServices;
using Ember.Memory;

namespace Ember.Utils;

public static unsafe class UnsafeUtils {

    private ref struct PtrRefReadonly<T>(ref readonly T* reference) where T : unmanaged {
        public ref readonly T* Reference = ref reference;
    }

    private ref struct PtrRef<T>(ref T* reference) where T : unmanaged {
        public ref T* Reference = ref reference;
    }

    private ref struct NIntRef(ref nint reference) {
        public ref nint Reference = ref reference;
    }

    private struct Obj(object? reference) {
        public object? Reference = reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly nint AsNIntRefReadonly<T>(ref readonly T* reference) where T : unmanaged {
        return ref Unsafe.BitCast<PtrRefReadonly<T>, NIntRef>(new(in reference)).Reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nint AsNIntRef<T>(ref T* reference) where T : unmanaged {
        return ref Unsafe.BitCast<PtrRef<T>, NIntRef>(new(ref reference)).Reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T* AsTPtrRefReadonly<T>(ref readonly nint reference) where T : unmanaged {
        return ref Unsafe.BitCast<NIntRef, PtrRefReadonly<T>>(new(ref Unsafe.AsRef(in reference))).Reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T* AsTPtrRef<T>(ref nint reference) where T : unmanaged {
        return ref Unsafe.BitCast<NIntRef, PtrRef<T>>(new(ref Unsafe.AsRef(in reference))).Reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint AsNInt<T>(T? reference) where T : class {
        return Unsafe.BitCast<Obj, nint>(new(reference));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint AsNInt<T>(ref readonly T reference) {
        return Unsafe.BitCast<RefReadonly<T>, nint>(new(in reference));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(nint ptr) {
        return ref Unsafe.BitCast<nint, Ref<T>>(ptr).Value;
    }

}
