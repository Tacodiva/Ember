using System;
using System.Runtime.CompilerServices;

namespace Ember.Utils;

public struct MemoryGuard {

    private const ulong MemoryGuardValue = 0x694206942069420;

    private ulong _value;

    public readonly bool IsValid {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return _value == MemoryGuardValue;
        }
    }

    public readonly ulong Value => _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Init() {
        _value = MemoryGuardValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Validate(string? guardingName = null) {
        if (!IsValid) {
            if (guardingName != null) throw new InvalidOperationException($"Memory guard '{guardingName}' was corrupted to 0x{_value:X}.");
            else throw new InvalidOperationException($"Memory guard was corrupted to 0x{_value:X}.");
        }
    }

}