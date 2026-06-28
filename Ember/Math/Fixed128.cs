using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Ember.Logging;

namespace Ember.Math;

public readonly struct Fixed128 :
    IAdditionOperators<Fixed128, Fixed128, Fixed128>, IAdditiveIdentity<Fixed128, Fixed128>, IDecrementOperators<Fixed128>,
    IEqualityOperators<Fixed128, Fixed128, bool>, IIncrementOperators<Fixed128>,
    IMultiplicativeIdentity<Fixed128, Fixed128>, ISubtractionOperators<Fixed128, Fixed128, Fixed128>,
    IUnaryNegationOperators<Fixed128, Fixed128>, IUnaryPlusOperators<Fixed128, Fixed128>,
    IEquatable<Fixed128>, IComparable<Fixed128>, IMinMaxValue<Fixed128> {

    public const int DenominatorBits = 16;
    public const int Denominator = 1 << DenominatorBits;
    public const long DenominatorMask = Denominator - 1;
    public const int IntegerBits = 128 - DenominatorBits;

    public static Fixed128 MaxValue => new(Int128.MaxValue);
    public static Fixed128 MinValue => new(Int128.MinValue);

    public static Fixed128 NegativeOne => -1;
    public static Fixed128 One => 1;

    public static int Radix => 2;

    public static Fixed128 Zero => 0;
    public static Fixed128 AdditiveIdentity => 0;
    public static Fixed128 MultiplicativeIdentity => 1;

    public readonly Int128 Data;


    public ulong Upper => Data.GetUpper();
    public ulong Lower => Data.GetLower();

    public Int128 IntegerPart {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return Data >> DenominatorBits;
        }
    }

    public int FractionalPart {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return (int)(Lower & DenominatorMask);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(ulong upper, ulong lower) {
        Data = new(upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(Int128 data) {
        Data = data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(decimal value) {
        decimal intPart = System.Math.Truncate(value);
        decimal fractionPart = value - intPart;

        if (decimal.IsNegative(intPart) && value != 0) {
            intPart -= 1;
            fractionPart += 1;
        }

        Data = (Int128)intPart;
        Data <<= DenominatorBits;
        Data |= (long)(fractionPart * Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(double value) {
        double intPart = System.Math.Truncate(value);
        double fractionPart = value - intPart;

        if (double.IsNegative(intPart) && value != 0) {
            intPart -= 1;
            fractionPart += 1;
        }

        Data = (Int128)intPart;
        Data <<= DenominatorBits;
        Data |= (long)(fractionPart * Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(float value) {
        float intPart = System.MathF.Truncate(value);
        float fractionPart = value - intPart;

        if (float.IsNegative(intPart) && value != 0) {
            intPart -= 1;
            fractionPart += 1;
        }

        Data = (Int128)intPart;
        Data <<= DenominatorBits;
        Data |= (long)(fractionPart * Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed128(ReadOnlySpan<byte> bytes) {
        Data = MemoryMarshal.Read<Int128>(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble() {
        return (double)IntegerPart + (FractionalPart / (double)Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToFloat() {
        return (float)IntegerPart + (FractionalPart / (float)Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ToDecimal() {
        return (decimal)IntegerPart + (FractionalPart / (decimal)Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Fixed128 other) {
        return Data.CompareTo(other.Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Fixed128 other) {
        return Data == other.Data;
    }

    public override bool Equals(object? obj) {
        if (obj is Fixed128 other) return Equals(other);
        return false;
    }

    public override int GetHashCode() {
        return Data.GetHashCode();
    }

    public override string ToString() {
        int fractionalPartInt = FractionalPart;
        if (fractionalPartInt == 0) {
            return IntegerPart.ToString();
        } else {

            decimal fractionalPartDecimal = FractionalPart / (decimal)Denominator;
            Int128 integerPart = IntegerPart;

            if (integerPart < 0) {
                fractionalPartDecimal = 1 - fractionalPartDecimal;

                if (integerPart == -1)
                    return string.Concat("-0.", fractionalPartDecimal.ToString().AsSpan(2));

                integerPart += 1;
            }

            return string.Concat(integerPart.ToString(), ".", fractionalPartDecimal.ToString().AsSpan(2));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(Fixed128 a, Fixed128 b) => new(a.Data + b.Data);

    public static Fixed128 Add(Fixed128 a, uint b) {
        ulong bShift = b << DenominatorBits;
        ulong lower = a.Lower + bShift;
        ulong upper = a.Upper;
        if (lower < a.Lower) ++upper;
        return new(upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(Fixed128 a, int b) => Add(a, (uint)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(int a, Fixed128 b) => Add(b, (uint)a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(Fixed128 a, uint b) => Add(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(uint a, Fixed128 b) => Add(b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(Fixed128 a, float b) => a + ((Fixed128)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(float a, Fixed128 b) => ((Fixed128)a) + b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(Fixed128 a, Fixed128 b) => new(a.Data - b.Data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 Subtract(Fixed128 a, uint b) {
        ulong lower = a.Lower - b;
        ulong upper = a.Upper;
        if (lower > a.Lower) --upper;
        return new(upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(Fixed128 a, int b) => Subtract(a, (uint)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(int a, Fixed128 b) => Subtract(b, (uint)a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(Fixed128 a, uint b) => Subtract(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(uint a, Fixed128 b) => Subtract(b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(Fixed128 a, float b) => a - ((Fixed128)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(float a, Fixed128 b) => ((Fixed128)a) - b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator --(Fixed128 value) => value - 1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator ++(Fixed128 value) => value + 1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator -(Fixed128 value) => new(-value.Data);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 operator +(Fixed128 value) => value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Fixed128 left, Fixed128 right) => left.Data > right.Data;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Fixed128 left, Fixed128 right) => left.Data >= right.Data;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Fixed128 left, Fixed128 right) => left.Data < right.Data;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Fixed128 left, Fixed128 right) => left.Data <= right.Data;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Fixed128 left, Fixed128 right) => left.Data == right.Data;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Fixed128 left, Fixed128 right) => left.Data != right.Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float(Fixed128 value) => value.ToFloat();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator double(Fixed128 value) => value.ToDouble();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator decimal(Fixed128 value) => value.ToDecimal();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(Fixed128 value) => (long)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(Fixed128 value) => (ulong)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(Fixed128 value) => (int)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Fixed128 value) => (uint)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator short(Fixed128 value) => (short)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ushort(Fixed128 value) => (ushort)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator byte(Fixed128 value) => (byte)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator sbyte(Fixed128 value) => (sbyte)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nint(Fixed128 value) => (nint)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nuint(Fixed128 value) => (nuint)(value.IntegerPart);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Int128(Fixed128 value) => value.IntegerPart;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(float value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Fixed128(double value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Fixed128(decimal value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(long value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(ulong value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(int value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(uint value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(short value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(ushort value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(byte value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(sbyte value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(nint value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fixed128(nuint value) => new((Int128)(value) << DenominatorBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Fixed128(Int128 value) => new(value << DenominatorBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 Floor(Fixed128 value) => new(value.Upper, value.Lower & ~(ulong)DenominatorMask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 Ceiling(Fixed128 value) {
        if (value.FractionalPart == 0) return value;
        return Floor(value) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 Round(Fixed128 value) {
        int roundType = value.FractionalPart.CompareTo(Denominator / 2);

        if (value < 0) {
            if (roundType <= 0) {
                return Floor(value);
            } else {
                return Ceiling(value);
            }
        } else {
            if (roundType < 0) {
                return Floor(value);
            } else {
                return Ceiling(value);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed128 Abs(Fixed128 value) => new(Int128.Abs(value.Data));

    public static bool IsCanonical(Fixed128 value) => true;

    public static bool IsComplexNumber(Fixed128 value) => false;

    public static bool IsEvenInteger(Fixed128 value) {
        return (value.Data & (1 << ((DenominatorBits + 1)) - 1)) == 0;
    }

    public static bool IsFinite(Fixed128 value) => true;
    public static bool IsImaginaryNumber(Fixed128 value) => false;
    public static bool IsInfinity(Fixed128 value) => false;
    public static bool IsNaN(Fixed128 value) => false;

    public static bool IsNegative(Fixed128 value) {
        return value.Data < 0;
    }

    public static bool IsInteger(Fixed128 value) {
        return (value.Data & DenominatorMask) == 0;
    }

    public static bool IsNegativeInfinity(Fixed128 value) => false;

    public static bool IsNormal(Fixed128 value) {
        return value != 0;
    }

    public static bool IsOddInteger(Fixed128 value) {
        return (value.Data & DenominatorMask) == 0 && (value.Data & (1 << DenominatorBits)) != 0;
    }

    public static bool IsPositive(Fixed128 value) {
        return value >= 0;
    }

    public static bool IsPositiveInfinity(Fixed128 value) => false;
    public static bool IsRealNumber(Fixed128 value) => true;
    public static bool IsSubnormal(Fixed128 value) => false;
    public static bool IsZero(Fixed128 value) => value == 0;

    public static Fixed128 MaxMagnitude(Fixed128 x, Fixed128 y) => new(Int128.MaxMagnitude(x.Data, y.Data));
    public static Fixed128 MaxMagnitudeNumber(Fixed128 x, Fixed128 y) => MaxMagnitude(x, y);

    public static Fixed128 MinMagnitude(Fixed128 x, Fixed128 y) => new(Int128.MinMagnitude(x.Data, y.Data));
    public static Fixed128 MinMagnitudeNumber(Fixed128 x, Fixed128 y) => MinMagnitude(x, y);
}
