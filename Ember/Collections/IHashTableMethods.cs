
using System.Runtime.CompilerServices;

namespace Ember.Collections;

public unsafe interface IUnmanagedHashTableMethods<TKey, TValue, TValueRef>
    where TKey : allows ref struct
    where TValue : unmanaged
    where TValueRef : allows ref struct {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetKeyHashCode(TKey key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract bool IsKeyValueEqual(TKey key, TValue* value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetValueHashCode(TValue* value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetValueRefHashCode(TValueRef valueRef);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract bool IsValueRefValueEqual(TValueRef valueRef, TValue* value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void DereferenceValue(TValueRef valueRef, TValue* destination);
}

public unsafe interface IHashTableMethods<TKey, TValue, TValueRef>
    where TKey : allows ref struct
    where TValueRef : allows ref struct {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetKeyHashCode(TKey key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract bool IsKeyValueEqual(TKey key, in TValue value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetValueHashCode(in TValue value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract int GetValueRefHashCode(TValueRef valueRef);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract bool IsValueRefValueEqual(TValueRef valueRef, in TValue value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void DereferenceValue(TValueRef valueRef, out TValue destination);
}