
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Ember.Collections;

public struct SingleSequence<T> : IEnumerable<T> {

    public readonly T Value;

    public SingleSequence(T value) {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(ref this);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {
        private readonly SingleSequence<T> _parent;
        private bool _couldMove;

        public Enumerator(ref SingleSequence<T> parent) {
            _parent = parent;
            _couldMove = true;
        }

        public T Current => _parent.Value;
        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext() {
            if (!_couldMove) return false;
            _couldMove = false;
            return true;
        }

        public void Reset() {
            _couldMove = true;
        }
    }
}