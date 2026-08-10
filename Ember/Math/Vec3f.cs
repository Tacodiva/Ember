
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Vec3f : IEquatable<Vec3f>, IFormattable {

    private Vector3 _internal;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f(float value) : this(new Vector3(value)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f(float x, float y, float z) : this(new Vector3(x, y, z)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f((int X, int Y, int Z) vec) : this(vec.X, vec.Y, vec.Z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f((float X, float Y, float Z) vec) : this(vec.X, vec.Y, vec.Z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f(Vector3 vector) {
        _internal = vector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f(ReadOnlySpan<float> values) : this(new Vector3(values)) { }

    public static Vec3f Zero => Vector3.Zero;

    public static Vec3f One => Vector3.One;

    public static Vec3f UnitX => Vector3.UnitX;

    public static Vec3f UnitY => Vector3.UnitY;

    public static Vec3f UnitZ => Vector3.UnitZ;

    public float this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _internal[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _internal[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator +(Vec3f left, Vec3f right) => left._internal + right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator /(Vec3f left, Vec3f right) => left._internal / right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator /(Vec3f left, float right) => left._internal / right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator /(float left, Vec3f right) => new(left / right.X, left / right.Y, left / right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3f left, Vec3f right) => left._internal * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3f left, float right) => left._internal * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(float left, Vec3f right) => left * right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3f left, Matrix4x4 right) => Vector3.Transform(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator -(Vec3f left, Vec3f right) => left._internal - right._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator -(Vec3f value) => new(-value.X, -value.Y, -value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3f left, Vec3b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0, right.Z ? left.Z : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3b left, Vec3f right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3f left, float right) => new(left.X == right, left.Y == right, left.Z == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3f left, float right) => new(left.X != right, left.Y != right, left.Z != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(float left, Vec3f right) => new(left == right.X, left == right.Y, left == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(float left, Vec3f right) => new(left != right.X, left != right.Y, left != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3f left, Vec3f right) => new(left.X == right.X, left.Y == right.Y, left.Z == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3f left, Vec3f right) => new(left.X != right.X, left.Y != right.Y, left.Z != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3f left, Vec3f right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3f left, float right) => new(left.X < right, left.Y < right, left.Z < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(float left, Vec3f right) => new(left < right.X, left < right.Y, left < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3f left, Vec3f right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3f left, float right) => new(left.X <= right, left.Y <= right, left.Z <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(float left, Vec3f right) => new(left <= right.X, left <= right.Y, left <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3f left, Vec3f right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3f left, float right) => new(left.X > right, left.Y > right, left.Z > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(float left, Vec3f right) => new(left > right.X, left > right.Y, left > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3f left, Vec3f right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3f left, float right) => new(left.X >= right, left.Y >= right, left.Z >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(float left, Vec3f right) => new(left >= right.X, left >= right.Y, left >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Floor(Vec3f value)
        => new(
            (int)System.Math.Floor(value.X),
            (int)System.Math.Floor(value.Y),
            (int)System.Math.Floor(value.Z)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i Ceiling(Vec3f value)
        => new(
            (int)System.Math.Ceiling(value.X),
            (int)System.Math.Ceiling(value.Y),
            (int)System.Math.Ceiling(value.Z)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Abs(Vec3f value) => Vector3.Abs(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Add(Vec3f left, Vec3f right) => Vector3.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Clamp(Vec3f value1, Vec3f min, Vec3f max) => Vector3.Clamp(value1, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Cross(Vec3f vector1, Vec3f vector2) => Vector3.Cross(vector1, vector2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vec3f value1, Vec3f value2) => Vector3.Distance(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec3f value1, Vec3f value2) => Vector3.DistanceSquared(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Divide(Vec3f left, float divisor) => Vector3.Divide(left, divisor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vec3f value1, Vec3f value2) => Vector3.Dot(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Lerp(Vec3f value1, Vec3f value2, float amount) => Vector3.Lerp(value1, value2, amount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Max(Vec3f value1, Vec3f value2) => Vector3.Max(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Min(Vec3f value1, Vec3f value2) => Vector3.Min(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Multiply(Vec3f left, Vec3f right) => Vector3.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Multiply(Vec3f left, float right) => Vector3.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Multiply(float left, Vec3f right) => Vector3.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Negate(Vec3f value) => Vector3.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Normalize(Vec3f value) => Vector3.Normalize(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Reflect(Vec3f value, Vec3f normal) => Vector3.Reflect(value, normal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f SquareRoot(Vec3f value) => Vector3.SquareRoot(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Subtract(Vec3f left, Vec3f right) => Vector3.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Transform(Vec3f position, Matrix4x4 matrix) => Vector3.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f Transform(Vec3f position, in Matrix4x4 matrix) => Vector3.Transform(position, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f TransformNormal(Vec3f normal, Matrix4x4 matrix) => Vector3.TransformNormal(normal, matrix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f TransformNormal(Vec3f normal, in Matrix4x4 matrix) => Vector3.TransformNormal(normal, matrix);

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
        if (obj is Vector3 otherVector3) return _internal.Equals(otherVector3);
        if (obj is Vec3f otherVector3f) return _internal.Equals(otherVector3f._internal);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec3f other) => _internal.Equals(other._internal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => _internal.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => _internal.Length();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float LengthSquared() => _internal.LengthSquared();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}f, {Y}f, {Z}f>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) => _internal.ToString(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider) => _internal.ToString(format, formatProvider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 AsSystem() => _internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f(Vector3 vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(Vec3f vec) => vec._internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (float X, float Y, float Z)(Vec3f vec) => (vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f((float X, float Y, float Z) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f((int X, int Y, int Z) vec) => new(vec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float x, out float y, out float z) {
        x = _internal.X;
        y = _internal.Y;
        z = _internal.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f((Vec2f XY, float Z) vec) => (vec.XY.X, vec.XY.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f((float X, Vec2f YZ) vec) => (vec.X, vec.YZ.X, vec.YZ.Y);

    #region Swizzels
    public Vec2f XY {
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

    public Vec2f XZ {
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

    public Vec2f YX {
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

    public Vec2f YZ {
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

    public Vec2f ZX {
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

    public Vec2f ZY {
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