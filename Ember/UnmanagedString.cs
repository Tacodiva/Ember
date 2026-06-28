
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ember.Math;

namespace Ember;

public unsafe readonly struct UnmanagedString : IEnumerable<char>, IEquatable<UnmanagedString>, IDisposable {

    public static UnmanagedString Allocate(string content) => new(Marshal.StringToHGlobalAnsi(content));

    public readonly byte* Handle;

    public bool IsNull => Handle == null;

    public UnmanagedString(nint handle) : this((byte*)handle) { }

    public UnmanagedString(byte* handle) {
        Handle = handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetLength() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        int i = 0;
        while (Handle[i] != 0) ++i;
        return i;
    }

    public override string ToString() {
#if EMBER_SAFETY_CHECKS
        if (IsNull) throw new NullReferenceException();
#endif
        return Marshal.PtrToStringAnsi((nint) Handle)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char CharAt(int index) {
        return (char)ByteAt(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ByteAt(int index) {
#if EMBER_SAFETY_CHECKS
        if (index < 0 || index >= GetLength())
            throw new IndexOutOfRangeException();
        if (IsNull) throw new NullReferenceException();
#endif
        return Handle[index];
    }

    public char this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CharAt(index);
    }

    public void Dispose() {
        Marshal.FreeHGlobal((nint) Handle);
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(string? other) {
        if (other == null) return IsNull;
        if (IsNull) return false;

        int i = 0;
        while (true) {
            byte b = Handle[i];
            if (b == 0) return other.Length == i;
            if (i == other.Length) return false;
            if (b != (byte)other[i]) return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UnmanagedString other) {
        if (other.IsNull) return IsNull;
        if (IsNull) return false;

        int i = 0;
        while (true) {
            byte aByte = Handle[i];
            if (aByte != other.Handle[i]) return false;
            if (aByte == 0) return true;
        }
    }

    public struct Enumerator : IEnumerator<char> {
        public readonly UnmanagedString String;
        public int Index { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator(UnmanagedString str) {
            String = str;
            Reset();
        }

        public readonly char Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return String.CharAt(Index);
            }
        }

        readonly object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            ++Index;
            byte b = String.Handle[Index];
            if (b == 0) {
                --Index;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => Index = -1;

        public readonly void Dispose() { }
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        return obj is UnmanagedString str && Equals(str);
    }

    public override readonly int GetHashCode() {
        if (IsNull) return 0;
        int hash = 0;
        int i = 0;
        while (true) {
            byte b = Handle[i];
            if (b == 0) return hash;
            hash = 31 * hash + b;
        }
    }

    public readonly Span<byte> AsSpan() {
        return new Span<byte>(Handle, GetLength());
    }

    public static implicit operator byte*(UnmanagedString value) => value.Handle;
    public static implicit operator void*(UnmanagedString value) => value.Handle;
    public static implicit operator nint(UnmanagedString value) => (nint) value.Handle;

    public static bool operator ==(UnmanagedString left, UnmanagedString right) => left.Equals(right);
    public static bool operator !=(UnmanagedString left, UnmanagedString right) => !left.Equals(right);
}