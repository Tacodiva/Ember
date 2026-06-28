
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Math;

public partial struct Vec4i : IEquatable<Vec4i>, IFormattable {

    public int X;
    public int Y;
    public int Z;
    public int W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i(int x, int y, int z, int w) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i(int value) : this(value, value, value, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i((int X, int Y, int Z, int W) vec) : this(vec.X, vec.Y, vec.Z, vec.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i(Vec3i xyz, int w) : this(xyz.X, xyz.Y, xyz.Z, w) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i(int x, Vec3i yzw) : this(x, yzw.X, yzw.Y, yzw.Z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4i(ReadOnlySpan<int> values) {
        if (values.Length < 4) throw new ArgumentOutOfRangeException(nameof(values));
        this = Unsafe.ReadUnaligned<Vec4i>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public static Vec4i Zero => default;

    public static Vec4i One => new(1);

    public static Vec4i UnitX => new(1, 0, 0, 0);

    public static Vec4i UnitY => new(0, 1, 0, 0);

    public static Vec4i UnitZ => new(0, 0, 1, 0);

    public static Vec4i UnitW => new(0, 0, 0, 1);

    public int this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            if (index == 0) return X;
            else if (index == 1) return Y;
            else if (index == 2) return Z;
            else if (index == 3) return W;
            else throw new ArgumentOutOfRangeException("index");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (index == 0) this.X = value;
            else if (index == 1) this.Y = value;
            else if (index == 2) this.Z = value;
            else if (index == 3) this.W = value;
            else throw new ArgumentOutOfRangeException("index");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator +(Vec4i left, Vec4i right) => new Vec4i(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator /(Vec4i left, Vec4i right) => new Vec4i(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator /(Vec4i left, int right) => new Vec4i(left.X / right, left.Y / right, left.Z / right, left.W / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator /(int left, Vec4i right) => new Vec4i(left / right.X, left / right.Y, left / right.Z, left / right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(Vec4i left, int right) => new(left.X == right, left.Y == right, left.Z == right, left.W == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(Vec4i left, int right) => new(left.X != right, left.Y != right, left.Z != right, left.W != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(int left, Vec4i right) => new(left == right.X, left == right.Y, left == right.Z, left == right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(int left, Vec4i right) => new(left != right.X, left != right.Y, left != right.Z, left != right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec4i left, Vec4i right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec4i left, Vec4i right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z || left.W != right.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(Vec4i left, Vec4i right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z, left.W < right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(Vec4i left, int right) => new(left.X < right, left.Y < right, left.Z < right, left.W < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(int left, Vec4i right) => new(left < right.X, left < right.Y, left < right.Z, left < right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(Vec4i left, Vec4i right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z, left.W <= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(Vec4i left, int right) => new(left.X <= right, left.Y <= right, left.Z <= right, left.W <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(int left, Vec4i right) => new(left <= right.X, left <= right.Y, left <= right.Z, left <= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(Vec4i left, Vec4i right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z, left.W > right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(Vec4i left, int right) => new(left.X > right, left.Y > right, left.Z > right, left.W > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(int left, Vec4i right) => new(left > right.X, left > right.Y, left > right.Z, left > right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(Vec4i left, Vec4i right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z, left.W >= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(Vec4i left, int right) => new(left.X >= right, left.Y >= right, left.Z >= right, left.W >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(int left, Vec4i right) => new(left >= right.X, left >= right.Y, left >= right.Z, left >= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(Vec4i left, Vec4i right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(Vec4i left, int right) => new(left.X * right, left.Y * right, left.Z * right, left.W * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(int left, Vec4i right) => new(left * right.X, left * right.Y, left * right.Z, left * right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator -(Vec4i left, Vec4i right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator -(Vec4i value) => new(-value.X, -value.Y, -value.Z, -value.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Abs(Vec4i value) => new Vec4i(System.Math.Abs(value.X), System.Math.Abs(value.Y), System.Math.Abs(value.Z), System.Math.Abs(value.W));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Add(Vec4i left, Vec4i right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Clamp(Vec4i value1, Vec4i min, Vec4i max)
        => new(
            System.Math.Clamp(value1.X, min.X, max.X),
            System.Math.Clamp(value1.Y, min.Y, max.Y),
            System.Math.Clamp(value1.Z, min.Z, max.Z),
            System.Math.Clamp(value1.W, min.W, max.W)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec4i value1, Vec4i value2) => Vector4.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec4i value1, Vec4i value2) => Vector4.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Divide(Vec4i left, int divisor) => left / divisor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Dot(Vec4i value1, Vec4i value2) => value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z + value1.W * value2.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Max(Vec4i value1, Vec4i value2)
        => new Vec4i(
            System.Math.Max(value1.X, value2.X),
            System.Math.Max(value1.Y, value2.Y),
            System.Math.Max(value1.Z, value2.Z),
            System.Math.Max(value1.W, value2.W)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Min(Vec4i value1, Vec4i value2)
        => new Vec4i(
            System.Math.Min(value1.X, value2.X),
            System.Math.Min(value1.Y, value2.Y),
            System.Math.Min(value1.Z, value2.Z),
            System.Math.Min(value1.W, value2.W)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Multiply(Vec4i left, Vec4i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Multiply(Vec4i left, int right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Multiply(int left, Vec4i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Negate(Vec4i value) => new(-value.X, -value.Y, -value.Z, -value.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Subtract(Vec4i left, Vec4i right) => left - right;

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
        if (destination.Length < 4) return false;
        destination[0] = X;
        destination[1] = Y;
        destination[2] = Z;
        destination[3] = W;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec4i otherVector2) return Equals(otherVector2);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec4i other) => this == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => MathF.Sqrt(LengthSquared());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int LengthSquared() => Dot(this, this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}, {Z}, {W}>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)}{separator} {W.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(Vec4i vec) => new(vec.X, vec.Y, vec.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f(Vec4i vec) => new(vec.X, vec.Y, vec.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4i((int X, int Y, int Z, int W) vec) => new(vec.X, vec.Y, vec.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec4i(Vec4f vec) => new((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out int x, out int y, out int z, out int w) {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }
}