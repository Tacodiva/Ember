
namespace Ember.Memory;

public unsafe interface IPtrWrapper {
    public void* Pointer { get; }
    public bool IsNull => Pointer == null;
}

public unsafe interface IPtrWrapper<T> : IPtrWrapper where T : unmanaged {
    void* IPtrWrapper.Pointer => Pointer;
    public new T* Pointer { get; }
}