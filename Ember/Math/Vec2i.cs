
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Math;

public struct Vec2i : IEquatable<Vec2i>, IFormattable {

    public int X;
    public int Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2i(int x, int y) {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2i(int value) : this(value, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2i((int X, int Y) vec) : this(vec.X, vec.Y) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2i(ReadOnlySpan<int> values) {
        if (values.Length < 2) throw new ArgumentOutOfRangeException(nameof(values));
        this = Unsafe.ReadUnaligned<Vec2i>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public static Vec2i Zero => default;

    public static Vec2i One => new(1, 1);

    public static Vec2i UnitX => new(1, 0);

    public static Vec2i UnitY => new(0, 1);

    public int this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            if (index == 0) return X;
            else if (index == 1) return Y;
            else throw new ArgumentOutOfRangeException("index");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (index == 0) this.X = value;
            else if (index == 1) this.Y = value;
            else throw new ArgumentOutOfRangeException("index");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator +(Vec2i left, Vec2i right) => new(left.X + right.X, left.Y + right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator /(Vec2i left, Vec2i right) => new(left.X / right.X, left.Y / right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator /(Vec2i left, int right) => new(left.X / right, left.Y / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator /(int left, Vec2i right) => new(left / right.X, left / right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(Vec2i left, Vec2i right) => new(left.X * right.X, left.Y * right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(Vec2i left, int right) => new(left.X * right, left.Y * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(int left, Vec2i right) => new(left * right.X, left * right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator -(Vec2i left, Vec2i right) => new(left.X - right.X, left.Y - right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator -(Vec2i value) => new(-value.X, -value.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(Vec2i left, Vec2b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(Vec2b left, Vec2i right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator <<(Vec2i left, Vec2i right) => new(left.X << right.X, left.Y << right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator <<(Vec2i left, int right) => new(left.X << right, left.Y << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator >>(Vec2i left, Vec2i right) => new(left.X >> right.X, left.Y >> right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator >>(Vec2i left, int right) => new(left.X >> right, left.Y >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator &(Vec2i left, Vec2i right) => new(left.X & right.X, left.Y & right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator &(Vec2i left, int right) => new(left.X & right, left.Y & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator |(Vec2i left, Vec2i right) => new(left.X | right.X, left.Y | right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator |(Vec2i left, int right) => new(left.X | right, left.Y | right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator ^(Vec2i left, Vec2i right) => new(left.X ^ right.X, left.Y ^ right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator ^(Vec2i left, int right) => new(left.X ^ right, left.Y ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator ~(Vec2i left) => new(~left.X, ~left.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(Vec2i left, int right) => new(left.X == right, left.Y == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(Vec2i left, int right) => new(left.X != right, left.Y != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(int left, Vec2i right) => new(left == right.X, left == right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(int left, Vec2i right) => new(left != right.X, left != right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec2i left, Vec2i right) => left.X == right.X && left.Y == right.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec2i left, Vec2i right) => left.X != right.X || left.Y != right.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(Vec2i left, Vec2i right) => new(left.X < right.X, left.Y < right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(Vec2i left, int right) => new(left.X < right, left.Y < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(int left, Vec2i right) => new(left < right.X, left < right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(Vec2i left, Vec2i right) => new(left.X <= right.X, left.Y <= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(Vec2i left, int right) => new(left.X <= right, left.Y <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(int left, Vec2i right) => new(left <= right.X, left <= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(Vec2i left, Vec2i right) => new(left.X > right.X, left.Y > right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(Vec2i left, int right) => new(left.X > right, left.Y > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(int left, Vec2i right) => new(left > right.X, left > right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(Vec2i left, Vec2i right) => new(left.X >= right.X, left.Y >= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(Vec2i left, int right) => new(left.X >= right, left.Y >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(int left, Vec2i right) => new(left >= right.X, left >= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Abs(Vec2i value) => new(System.Math.Abs(value.X), System.Math.Abs(value.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Add(Vec2i left, Vec2i right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Clamp(Vec2i value1, Vec2i min, Vec2i max)
        => new(System.Math.Clamp(value1.X, min.X, max.X), System.Math.Clamp(value1.Y, min.Y, max.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec2i value1, Vec2i value2) => Vector2.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec2i value1, Vec2i value2) => Vector2.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Divide(Vec2i left, int divisor) => left / divisor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Dot(Vec2i value1, Vec2i value2) => value1.X * value2.X + value1.Y * value2.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Max(Vec2i value1, Vec2i value2)
        => new(System.Math.Max(value1.X, value2.X), System.Math.Max(value1.Y, value2.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Min(Vec2i value1, Vec2i value2)
        => new(System.Math.Min(value1.X, value2.X), System.Math.Min(value1.Y, value2.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Multiply(Vec2i left, Vec2i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Multiply(Vec2i left, int right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Multiply(int left, Vec2i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Negate(Vec2i value) => new(-value.X, -value.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Subtract(Vec2i left, Vec2i right) => left - right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(int[] array) => CopyTo(array, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(int[] array, int index) => CopyTo(array.AsSpan()[index..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Span<int> destination) {
        if (!TryCopyTo(destination)) throw new ArgumentOutOfRangeException(nameof(destination));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryCopyTo(Span<int> destination) {
        if (destination.Length < 2) return false;
        destination[0] = X;
        destination[1] = Y;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec2i otherVector2) return Equals(otherVector2);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec2i other) => this == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => MathF.Sqrt(LengthSquared());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int LengthSquared() => Dot(this, this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float AspectRatio() => (float)X / (float)Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(Vec2i vec) => new(vec.X, vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2f(Vec2i vec) => new(vec.X, vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2i((int X, int Y) vec) => new(vec.X, vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec2i(Vec2f vec) => new((int)vec.X, (int)vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out int x, out int y) {
        x = X;
        y = Y;
    }
}