
using System;
using System.Runtime.InteropServices;

namespace Ember.Utils;

public readonly struct DelegatePtr<T>(nint handle) where T : Delegate {
    public static DelegatePtr<T> Create(T @delegate) {
        return new(@delegate);
    }

    public readonly nint Handle = handle;

    public DelegatePtr(T @delegate) : this(Marshal.GetFunctionPointerForDelegate(@delegate)) { }

    public T Get() {
        return Marshal.GetDelegateForFunctionPointer<T>(Handle);
    }

    public static explicit operator DelegatePtr<T>(nint ptr) => new(ptr);
    public static implicit operator nint(DelegatePtr<T> ptr) => ptr.Handle;
}