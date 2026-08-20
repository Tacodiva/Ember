
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Vec2f : IEquatable<Vec2f>, IFormattable {

    private Vector2 _internal;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2f(Vector2 vec) {
        _internal = vec;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2f(float x, float y) : this(new Vector2(x, y)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2f((float X, float Y) vec) : this(new Vector2(vec.X, vec.Y)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2f((int X, int Y) vec) : this(new Vector2(vec.X, vec.Y)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2f(ReadOnlySpan<float> values) : this(new Vector2(values)) { }

    #region Copied Methods
    public static Vec2f Zero => Vector2.Zero;

    public static Vec2f One => Vector2.One;

    public static Vec2f UnitX => Vector2.UnitX;

    public static Vec2f UnitY => Vector2.UnitY;

    public float this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _internal[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _internal[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator +(Vec2f left, Vec2f right) => left._internal + right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator /(Vec2f left, Vec2f right) => left._internal / right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator /(Vec2f left, float right) => left._internal / right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator /(float left, Vec2f right) => new(left / right.X, left / right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(Vec2f left, Vec2f right) => left._internal * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(Vec2f left, float right) => left._internal * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(float left, Vec2f right) => left * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator -(Vec2f left, Vec2f right) => left._internal - right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator -(Vec2f value) => new(-value.X, -value.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(Vec2f left, Vec2b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(Vec2b left, Vec2f right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(Vec2f left, float right) => new(left.X == right, left.Y == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(Vec2f left, float right) => new(left.X != right, left.Y != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(float left, Vec2f right) => new(left == right.X, left == right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(float left, Vec2f right) => new(left != right.X, left != right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec2f left, Vec2f right) => left._internal == right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec2f left, Vec2f right) => left._internal != right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(Vec2f left, Vec2f right) => new(left.X < right.X, left.Y < right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(Vec2f left, float right) => new(left.X < right, left.Y < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <(float left, Vec2f right) => new(left < right.X, left < right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(Vec2f left, Vec2f right) => new(left.X <= right.X, left.Y <= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(Vec2f left, float right) => new(left.X <= right, left.Y <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator <=(float left, Vec2f right) => new(left <= right.X, left <= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(Vec2f left, Vec2f right) => new(left.X > right.X, left.Y > right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(Vec2f left, float right) => new(left.X > right, left.Y > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >(float left, Vec2f right) => new(left > right.X, left > right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(Vec2f left, Vec2f right) => new(left.X >= right.X, left.Y >= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(Vec2f left, float right) => new(left.X >= right, left.Y >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator >=(float left, Vec2f right) => new(left >= right.X, left >= right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Floor(Vec2f value) => new((int)System.Math.Floor(value.X), (int)System.Math.Floor(value.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i Ceiling(Vec2f value) => new((int)System.Math.Ceiling(value.X), (int)System.Math.Ceiling(value.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Abs(Vec2f value) => Vector2.Abs(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Add(Vec2f left, Vec2f right) => Vector2.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Clamp(Vec2f value1, Vec2f min, Vec2f max) => Vector2.Clamp(value1, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec2f value1, Vec2f value2) => Vector2.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec2f value1, Vec2f value2) => Vector2.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Divide(Vec2f left, float divisor) => Vector2.Divide(left, divisor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vec2f value1, Vec2f value2) => Vector2.Dot(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Lerp(Vec2f value1, Vec2f value2, float amount) => Vector2.Lerp(value1, value2, amount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Max(Vec2f value1, Vec2f value2) => Vector2.Max(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Min(Vec2f value1, Vec2f value2) => Vector2.Min(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Multiply(Vec2f left, Vec2f right) => Vector2.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Multiply(Vec2f left, float right) => Vector2.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Multiply(float left, Vec2f right) => Vector2.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Negate(Vec2f value) => Vector2.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Normalize(Vec2f value) => Vector2.Normalize(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Reflect(Vec2f value, Vec2f normal) => Vector2.Reflect(value, normal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f SquareRoot(Vec2f value) => Vector2.SquareRoot(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Subtract(Vec2f left, Vec2f right) => Vector2.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Transform(Vec2f position, Matrix4x4 matrix) => Vector2.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Transform(Vec2f position, in Matrix4x4 matrix) => Vector2.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f TransformNormal(Vec2f normal, Matrix4x4 matrix) => Vector2.TransformNormal(normal, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f TransformNormal(Vec2f normal, in Matrix4x4 matrix) => Vector2.TransformNormal(normal, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Transform(Vec2f position, Matrix3x2 matrix) => Vector2.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f Transform(Vec2f position, in Matrix3x2 matrix) => Vector2.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f TransformNormal(Vec2f normal, Matrix3x2 matrix) => Vector2.TransformNormal(normal, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f TransformNormal(Vec2f normal, in Matrix3x2 matrix) => Vector2.TransformNormal(normal, matrix);

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
        if (obj is Vector2 otherVector2) return _internal.Equals(otherVector2);
        if (obj is Vec2f otherVector2f) return _internal.Equals(otherVector2f._internal);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec2f other) => _internal.Equals(other._internal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => _internal.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => _internal.Length();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float LengthSquared() => _internal.LengthSquared();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float AspectRatio() => _internal.X / _internal.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}f, {Y}f>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) => _internal.ToString(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) => _internal.ToString(format, formatProvider);
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(Vec2f vec) => vec._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2f(Vector2 vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2f((int X, int Y) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2f((float X, float Y) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float x, out float y) {
        x = _internal.X;
        y = _internal.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f UnitVector(float angle) {
        return new(MathF.Sin(angle), MathF.Cos(angle));
    }
}