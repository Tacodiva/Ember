
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Ember.Collections;

namespace Ember.Collections;

public readonly unsafe struct UnmanagedIndexAllocator : IEnumerable<int>, IDisposable {

    private const int BitsPerBitfieldValue = sizeof(ulong) * 8;
    private const int BitsPerBitfieldValueLog2 = 6; // log2(64) = 6
    private const int BitIndexMask = (1 << BitsPerBitfieldValueLog2) - 1;

    public static UnmanagedIndexAllocator Allocate() {
        return new(UnmanagedList<ulong>.Allocate());
    }

    /// <summary>
    /// A bitfield where 1 indicates the corresponding index is free index and 0 indicates it is occupied. 
    /// </summary>
    private readonly UnmanagedList<ulong> _indexBitfield;
    public bool IsNull => _indexBitfield.IsNull;

    private UnmanagedIndexAllocator(UnmanagedList<ulong> indexBitmap) {
        _indexBitfield = indexBitmap;
    }

    [Conditional("EMBER_SAFETY_CHECKS")]
    internal void ValidateSelf() {
        if (_indexBitfield.IsNull) throw new NullReferenceException($"{nameof(UnmanagedIndexAllocator)} is null.");
        _indexBitfield.ValidateSelf();
    }

    public int AllocateIndex() {
        ValidateSelf();

        int initialBitfieldLength = _indexBitfield.Count;

        // Search for a free index in the bitmap
        {
            ulong* indexBitfield = _indexBitfield.BasePointer;

            for (int i = 0; i < initialBitfieldLength; i++) {
                ulong indexBitfieldValue = indexBitfield[i];

                // If the bitfield value is 0 there is no avaliable indices.
                if (indexBitfieldValue != 0) {

                    // Find the index of the first 1
                    int bitIdx = BitOperations.TrailingZeroCount(indexBitfieldValue);

                    // Set the 1 at bitIdx to 0
                    indexBitfield[i] = indexBitfieldValue & ~(1UL << bitIdx);

                    // Return the index we've just marked as allocated
                    return (i << BitsPerBitfieldValueLog2) | bitIdx;

                }
            }
        }

        // If that failed, add a new ulong to the bitmap

        // We want to mark the first index in the new value as occupied.
        _indexBitfield.Add(~0x1UL);

        return initialBitfieldLength << BitsPerBitfieldValueLog2;
    }

    public void FreeIndex(int index) {
        ValidateSelf();

        int bitfieldValueIndex = index >> BitsPerBitfieldValueLog2;
        int bitIndex = index & BitIndexMask;

#if EMBER_SAFETY_CHECKS
        if (bitfieldValueIndex >= _indexBitfield.Count) throw new ArgumentException($"Index {index} never allocated (bitfield out of bounds).");
#endif

        ulong* bitfieldValue = &_indexBitfield.BasePointer[bitfieldValueIndex];

#if EMBER_SAFETY_CHECKS
        if ((*bitfieldValue & (1UL << bitIndex)) != 0) throw new ArgumentException($"Index {index} never allocated (bitfield set to 0).");
#endif

        *bitfieldValue |= (1UL << bitIndex);
    }

    public bool IsIndexAllocated(int index) {
        ValidateSelf();

        int bitfieldValueIndex = index >> BitsPerBitfieldValueLog2;

        if (bitfieldValueIndex >= _indexBitfield.Count) return false;

        int bitIndex = index & BitIndexMask;

        return (_indexBitfield.BasePointer[bitfieldValueIndex] & (1UL << bitIndex)) == 0;
    }

    public void Clear() {
        ValidateSelf();
        _indexBitfield.Clear();
    }

    public void Dispose() {
        ValidateSelf();
        _indexBitfield.Dispose();
    }

    public Enumerator GetEnumerator() {
        return new(_indexBitfield.GetEnumerator());
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator(UnmanagedInlineList<ulong>.Enumerator bitfieldEnumerator) : IEnumerator<int> {
        private UnmanagedInlineList<ulong>.Enumerator _bitfieldEnumerator = bitfieldEnumerator;
        private ulong _currentBitfield = ulong.MaxValue;
        private int _currentBitfieldIndex = -1;
        private int _currentBitIndex;

        public readonly int Current => _currentBitfieldIndex << BitsPerBitfieldValueLog2 | _currentBitIndex;
        readonly object IEnumerator.Current => Current;

        public bool MoveNext() {
            if (_currentBitfield == ulong.MaxValue) {
                while (_currentBitfield == ulong.MaxValue) {
                    if (!_bitfieldEnumerator.MoveNext())
                        return false;
                    _currentBitfield = _bitfieldEnumerator.Current;
                    ++_currentBitfieldIndex;
                }
                _currentBitIndex = 0;
            }

            // Find the index of the first 0
            _currentBitIndex = BitOperations.TrailingZeroCount(~_currentBitfield);

            // Set the 0 at bitIdx to a 1
            _currentBitfield |= 1UL << _currentBitIndex;

            return true;
        }

        public void Reset() {
            _bitfieldEnumerator.Reset();
            _currentBitfield = ulong.MaxValue;
            _currentBitfieldIndex = -1;
            _currentBitIndex = BitsPerBitfieldValueLog2;
        }

        public void Dispose() {
            _bitfieldEnumerator.Dispose();
        }
    }
}