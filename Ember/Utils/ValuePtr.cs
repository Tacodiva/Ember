using System.Runtime.CompilerServices;

namespace Ember.Utils;

internal unsafe readonly struct ValuePtr<T>(nint value) where T : unmanaged {
    private readonly nint _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValuePtr<T> Create(in T value) {
        ValuePtr<T> reference;
        if (sizeof(T) <= sizeof(nint)) *(T*)&reference = value;
        else *(T**)&reference = (T*)Unsafe.AsPointer(ref Unsafe.AsRef<T>(in value));
        return reference;
    }

    public static ValuePtr<T> Create(T* value) {
        ValuePtr<T> reference;
        if (sizeof(T) <= sizeof(nint)) *(T*)&reference = *value;
        else *(T**)&reference = value;
        return reference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValuePtr<T>(T* pointer) => Create(pointer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(ValuePtr<T> reference) => reference.Get();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get() {
        nint value = _value;
        if (sizeof(T) <= sizeof(nint)) return *((T*)&value);
        else return *(T*)_value;
    }
}