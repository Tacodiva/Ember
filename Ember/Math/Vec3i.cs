
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Math;

public struct Vec3i : IEquatable<Vec3i>, IFormattable {

    public int X;
    public int Y;
    public int Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i(int x, int y, int z) {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i(int value) : this(value, value, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i((int X, int Y, int Z) vec) : this(vec.X, vec.Y, vec.Z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i(Vec2i xy, int z) : this(xy.X, xy.Y, z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i(int x, Vec2i yz) : this(x, yz.X, yz.Y) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3i(ReadOnlySpan<int> values) {
        if (values.Length < 3) throw new ArgumentOutOfRangeException(nameof(values));
        this = Unsafe.ReadUnaligned<Vec3i>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public static Vec3i Zero => default;

    public static Vec3i One => new(1);

    public static Vec3i UnitX => new(1, 0, 0);

    public static Vec3i UnitY => new(0, 1, 0);

    public static Vec3i UnitZ => new(0, 0, 1);

    public int this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            if (index == 0) return X;
            else if (index == 1) return Y;
            else if (index == 2) return Z;
            else throw new ArgumentOutOfRangeException("index");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (index == 0) this.X = value;
            else if (index == 1) this.Y = value;
            else if (index == 2) this.Z = value;
            else throw new ArgumentOutOfRangeException("index");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator +(Vec3i left, Vec3i right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator /(Vec3i left, Vec3i right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator /(Vec3i left, int right) => new(left.X / right, left.Y / right, left.Z / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator /(int left, Vec3i right) => new(left / right.X, left / right.Y, left / right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator <<(Vec3i left, int right) => new(left.X << right, left.Y << right, left.Z << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator <<(Vec3i left, Vec3i right) => new(left.X << right.X, left.Y << right.Y, left.Z << right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator >>(Vec3i left, int right) => new(left.X >> right, left.Y >> right, left.Z >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator >>(Vec3i left, Vec3i right) => new(left.X >> right.X, left.Y >> right.Y, left.Z >> right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator |(Vec3i left, int right) => new(left.X | right, left.Y | right, left.Z | right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator |(Vec3i left, Vec3i right) => new(left.X | right.X, left.Y | right.Y, left.Z | right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator &(Vec3i left, int right) => new(left.X & right, left.Y & right, left.Z & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator &(Vec3i left, Vec3i right) => new(left.X & right.X, left.Y & right.Y, left.Z & right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(Vec3i left, Vec3i right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(Vec3i left, int right) => new(left.X * right, left.Y * right, left.Z * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(int left, Vec3i right) => new(left * right.X, left * right.Y, left * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator -(Vec3i left, Vec3i right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator -(Vec3i value) => new(-value.X, -value.Y, -value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(Vec3i left, Vec3b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0, right.Z ? left.Z : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(Vec3b left, Vec3i right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator ^(Vec3i left, Vec3i right) => new(left.X ^ right.X, left.Y ^ right.Y, left.Z ^ right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator ^(Vec3i left, int right) => new(left.X ^ right, left.Y ^ right, left.Z ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator ~(Vec3i left) => new(~left.X, ~left.Y, ~left.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3i left, int right) => new(left.X == right, left.Y == right, left.Z == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3i left, int right) => new(left.X != right, left.Y != right, left.Z != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(int left, Vec3i right) => new(left == right.X, left == right.Y, left == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(int left, Vec3i right) => new(left != right.X, left != right.Y, left != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3i left, Vec3i right) => new(left.X == right.X, left.Y == right.Y, left.Z == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3i left, Vec3i right) => new(left.X != right.X, left.Y != right.Y, left.Z != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3i left, Vec3i right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3i left, int right) => new(left.X < right, left.Y < right, left.Z < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(int left, Vec3i right) => new(left < right.X, left < right.Y, left < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3i left, Vec3i right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3i left, int right) => new(left.X <= right, left.Y <= right, left.Z <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(int left, Vec3i right) => new(left <= right.X, left <= right.Y, left <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3i left, Vec3i right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3i left, int right) => new(left.X > right, left.Y > right, left.Z > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(int left, Vec3i right) => new(left > right.X, left > right.Y, left > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3i left, Vec3i right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3i left, int right) => new(left.X >= right, left.Y >= right, left.Z >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(int left, Vec3i right) => new(left >= right.X, left >= right.Y, left >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Abs(Vec3i value) => new(System.Math.Abs(value.X), System.Math.Abs(value.Y), System.Math.Abs(value.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Add(Vec3i left, Vec3i right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Clamp(Vec3i value1, Vec3i min, Vec3i max)
        => new(
            System.Math.Clamp(value1.X, min.X, max.X),
            System.Math.Clamp(value1.Y, min.Y, max.Y),
            System.Math.Clamp(value1.Z, min.Z, max.Z)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Cross(Vec3i vector1, Vec3i vector2)
        => new(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec3i value1, Vec3i value2) => Vector3.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec3i value1, Vec3i value2) => Vector3.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Divide(Vec3i left, int divisor) => left / divisor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Dot(Vec3i value1, Vec3i value2) => value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Max(Vec3i value1, Vec3i value2)
        => new(
            System.Math.Max(value1.X, value2.X),
            System.Math.Max(value1.Y, value2.Y),
            System.Math.Max(value1.Z, value2.Z)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Min(Vec3i value1, Vec3i value2)
        => new(
            System.Math.Min(value1.X, value2.X),
            System.Math.Min(value1.Y, value2.Y),
            System.Math.Min(value1.Z, value2.Z)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Multiply(Vec3i left, Vec3i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Multiply(Vec3i left, int right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Multiply(int left, Vec3i right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Negate(Vec3i value) => new(-value.X, -value.Y, -value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Subtract(Vec3i left, Vec3i right) => left - right;

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
        if (destination.Length < 3) return false;
        destination[0] = X;
        destination[1] = Y;
        destination[2] = Z;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec3i otherVector2) return Equals(otherVector2);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec3i other) => (this == other).All();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => MathF.Sqrt(LengthSquared());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int LengthSquared() => Dot(this, this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}, {Z}>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(Vec3i vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f(Vec3i vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3i(Vec3f vec) => new((int)vec.X, (int)vec.Y, (int)vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out int x, out int y, out int z) {
        x = X;
        y = Y;
        z = Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3i((int X, int Y, int Z) vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3i((Vec2i XY, int Z) vec) => new(vec.XY, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3i((int X, Vec2i YZ) vec) => new(vec.X, vec.YZ);

    #region Swizzels
    public Vec2i XY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            X = value.X;
            Y = value.Y;
        }
    }

    public Vec2i XZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            X = value.X;
            Z = value.Y;
        }
    }

    public Vec2i YX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Y = value.X;
            X = value.Y;
        }
    }

    public Vec2i YZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Y = value.X;
            Z = value.Y;
        }
    }

    public Vec2i ZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Z = value.X;
            X = value.Y;
        }
    }

    public Vec2i ZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return new(Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Z = value.X;
            Y = value.Y;
        }
    }

    #endregion
}