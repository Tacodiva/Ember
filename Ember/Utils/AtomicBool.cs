
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ember.Utils;

public struct AtomicBool {
    public const int True = 1;
    public const int False = 0;

    private volatile int _value;

    public bool Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _value == True;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _value = value ? True : False;
    }

    public AtomicBool(bool value) {
        Value = value;
    }

    public bool TrueToFalse() {
        int result = Interlocked.CompareExchange(ref _value, False, True);
        return result == True;
    }

    public bool FalseToTrue() {
        int result = Interlocked.CompareExchange(ref _value, True, False);
        return result == False;
    }

    public static implicit operator bool(AtomicBool atomic) => atomic.Value;
    public static implicit operator AtomicBool(bool value) => new(value);

    public override readonly string ToString() {
        return Value.ToString();
    }
}