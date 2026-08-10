
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public partial struct Vec4f : IEquatable<Vec4f>, IFormattable {

    private Vector4 _internal;

    public float X {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return _internal.X;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            _internal.X = value;
        }
    }

    public float Y {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return _internal.Y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            _internal.Y = value;
        }
    }

    public float Z {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return _internal.Z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            _internal.Z = value;
        }
    }

    public float W {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return _internal.W;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            _internal.W = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(float value) : this(new Vector4(value)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(float x, float y, float z, float w) : this(new Vector4(x, y, z, w)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f((float X, float Y, float Z, float W) vec) : this(vec.X, vec.Y, vec.Z, vec.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f((int X, int Y, int Z, int W) vec) : this(vec.X, vec.Y, vec.Z, vec.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(Vector4 vector) {
        _internal = vector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(Vec3f xyz, float w) : this(new Vector4(xyz, w)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(float x, Vec3f yzw) : this(new Vector4(x, yzw.X, yzw.Y, yzw.Z)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4f(ReadOnlySpan<float> values) : this(new Vector4(values)) { }

    #region Copied Methods
    public static Vec4f Zero => Vector4.Zero;

    public static Vec4f One => Vector4.One;

    public static Vec4f UnitX => Vector4.UnitX;

    public static Vec4f UnitY => Vector4.UnitY;

    public static Vec4f UnitZ => Vector4.UnitZ;

    public static Vec4f UnitW => Vector4.UnitW;

    public float this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _internal[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _internal[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator +(Vec4f left, Vec4f right) => left._internal + right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator /(Vec4f left, Vec4f right) => left._internal / right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator /(Vec4f left, float right) => left._internal / right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator /(float left, Vec4f right) => new(left / right.X, left / right.Y, left / right.Z, left / right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(Vec4f left, Vec4f right) => left._internal * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(Vec4f left, float right) => left._internal * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(float left, Vec4f right) => left * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator -(Vec4f left, Vec4f right) => left._internal - right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator -(Vec4f value) => new(-value.X, -value.Y, -value.Z, -value.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(Vec4f left, Vec4b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0, right.Z ? left.Z : 0, right.W ? left.W : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(Vec4b left, Vec4f right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(Vec4f left, float right) => new(left.X == right, left.Y == right, left.Z == right, left.W == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(Vec4f left, float right) => new(left.X != right, left.Y != right, left.Z != right, left.W != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(float left, Vec4f right) => new(left == right.X, left == right.Y, left == right.Z, left == right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(float left, Vec4f right) => new(left != right.X, left != right.Y, left != right.Z, left != right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(Vec4f left, Vec4f right) => new(left.X == right.X, left.Y == right.Y, left.Z == right.Z, left.W == right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(Vec4f left, Vec4f right) => new(left.X != right.X, left.Y != right.Y, left.Z != right.Z, left.W != right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(Vec4f left, Vec4f right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z, left.W < right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(Vec4f left, float right) => new(left.X < right, left.Y < right, left.Z < right, left.W < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <(float left, Vec4f right) => new(left < right.X, left < right.Y, left < right.Z, left < right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(Vec4f left, Vec4f right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z, left.W <= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(Vec4f left, float right) => new(left.X <= right, left.Y <= right, left.Z <= right, left.W <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator <=(float left, Vec4f right) => new(left <= right.X, left <= right.Y, left <= right.Z, left <= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(Vec4f left, Vec4f right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z, left.W > right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(Vec4f left, float right) => new(left.X > right, left.Y > right, left.Z > right, left.W > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >(float left, Vec4f right) => new(left > right.X, left > right.Y, left > right.Z, left > right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(Vec4f left, Vec4f right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z, left.W >= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(Vec4f left, float right) => new(left.X >= right, left.Y >= right, left.Z >= right, left.W >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator >=(float left, Vec4f right) => new(left >= right.X, left >= right.Y, left >= right.Z, left >= right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Floor(Vec4f value)
        => new(
            (int)System.Math.Floor(value.X),
            (int)System.Math.Floor(value.Y),
            (int)System.Math.Floor(value.Z),
            (int)System.Math.Floor(value.W)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i Ceiling(Vec4f value)
        => new(
            (int)System.Math.Ceiling(value.X),
            (int)System.Math.Ceiling(value.Y),
            (int)System.Math.Ceiling(value.Z),
            (int)System.Math.Ceiling(value.W)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Abs(Vec4f value) => Vector4.Abs(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Add(Vec4f left, Vec4f right) => Vector4.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Clamp(Vec4f value1, Vec4f min, Vec4f max) => Vector4.Clamp(value1, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec4f value1, Vec4f value2) => Vector4.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec4f value1, Vec4f value2) => Vector4.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Divide(Vec4f left, float divisor) => Vector4.Divide(left, divisor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vec4f value1, Vec4f value2) => Vector4.Dot(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Lerp(Vec4f value1, Vec4f value2, float amount) => Vector4.Lerp(value1, value2, amount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Max(Vec4f value1, Vec4f value2) => Vector4.Max(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Min(Vec4f value1, Vec4f value2) => Vector4.Min(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Multiply(Vec4f left, Vec4f right) => Vector4.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Multiply(Vec4f left, float right) => Vector4.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Multiply(float left, Vec4f right) => Vector4.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Negate(Vec4f value) => Vector4.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Normalize(Vec4f value) => Vector4.Normalize(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f SquareRoot(Vec4f value) => Vector4.SquareRoot(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Subtract(Vec4f left, Vec4f right) => Vector4.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Transform(Vec4f position, Matrix4x4 matrix) => Vector4.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f Transform(Vec4f position, in Matrix4x4 matrix) => Vector4.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(float[] array) => _internal.CopyTo(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(float[] array, int index) => _internal.CopyTo(array, index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Span<float> destination) => _internal.CopyTo(destination);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryCopyTo(Span<float> destination) => _internal.TryCopyTo(destination);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vector4 otherVector4) return _internal.Equals(otherVector4);
        if (obj is Vec4f otherVector4f) return _internal.Equals(otherVector4f._internal);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec4f other) => _internal.Equals(other._internal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => _internal.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => _internal.Length();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float LengthSquared() => _internal.LengthSquared();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}f, {Y}f, {Z}f, {W}f>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) => _internal.ToString(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) => _internal.ToString(format, formatProvider);
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 AsSystem() => _internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f(Vector4 vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(Vec4f vec) => vec._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (float X, float Y, float Z)(Vec4f vec) => (vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f((float X, float Y, float Z, float W) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f((int X, int Y, int Z, int W) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f((float X, Vec3f YZW) vec) => new(vec.X, vec.YZW);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f((Vec3f XYZ, float W) vec) => new(vec.XYZ, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float x, out float y, out float z, out float w) {
        x = _internal.X;
        y = _internal.Y;
        z = _internal.Z;
        w = _internal.W;
    }

}