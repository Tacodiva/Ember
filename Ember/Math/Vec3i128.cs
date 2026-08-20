
using System;
using System.Runtime.CompilerServices;
using Ember.Math;

namespace Ember.Math;

public struct Vec3i128 : IEquatable<Vec3i128> {

    public Int128 X, Y, Z;

    public Vec3i128(Int128 x, Int128 y, Int128 z) {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator +(Vec3i128 left, Vec3i128 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator +(Vec3i128 left, Vec3i right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator +(Vec3i left, Vec3i128 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator /(Vec3i128 left, Vec3i128 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator /(Vec3i128 left, int right) => new(left.X / right, left.Y / right, left.Z / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator /(int left, Vec3i128 right) => new(left / right.X, left / right.Y, left / right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator <<(Vec3i128 left, int right) => new(left.X << right, left.Y << right, left.Z << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator <<(Vec3i128 left, Vec3i right) => new(left.X << right.X, left.Y << right.Y, left.Z << right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator >>(Vec3i128 left, int right) => new(left.X >> right, left.Y >> right, left.Z >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator >>(Vec3i128 left, Vec3i right) => new(left.X >> right.X, left.Y >> right.Y, left.Z >> right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator |(Vec3i128 left, int right) => new(left.X | right, left.Y | right, left.Z | right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator |(Vec3i128 left, Vec3i128 right) => new(left.X | right.X, left.Y | right.Y, left.Z | right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator &(Vec3i128 left, int right) => new(left.X & right, left.Y & right, left.Z & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator &(Vec3i128 left, Vec3i128 right) => new(left.X & right.X, left.Y & right.Y, left.Z & right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3i128 left, Vec3i128 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3i left, Vec3i128 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3i128 left, Vec3i right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3i128 left, int right) => new(left.X * right, left.Y * right, left.Z * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(int left, Vec3i128 right) => new(left * right.X, left * right.Y, left * right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator -(Vec3i128 left, Vec3i128 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator -(Vec3i left, Vec3i128 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator -(Vec3i128 left, Vec3i right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator -(Vec3i128 value) => new(-value.X, -value.Y, -value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3i128 left, Vec3b right) => new(right.X ? left.X : 0, right.Y ? left.Y : 0, right.Z ? left.Z : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator *(Vec3b left, Vec3i128 right) => right * left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator ^(Vec3i128 left, Vec3i128 right) => new(left.X ^ right.X, left.Y ^ right.Y, left.Z ^ right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator ^(Vec3i128 left, int right) => new(left.X ^ right, left.Y ^ right, left.Z ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i128 operator ~(Vec3i128 left) => new(~left.X, ~left.Y, ~left.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3i128 left, int right) => new(left.X == right, left.Y == right, left.Z == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3i128 left, int right) => new(left.X != right, left.Y != right, left.Z != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(int left, Vec3i128 right) => new(left == right.X, left == right.Y, left == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(int left, Vec3i128 right) => new(left != right.X, left != right.Y, left != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec3i128 left, Vec3i128 right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec3i128 left, Vec3i128 right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3i128 left, Vec3i128 right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3i128 left, int right) => new(left.X < right, left.Y < right, left.Z < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(int left, Vec3i128 right) => new(left < right.X, left < right.Y, left < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3i128 left, Vec3i128 right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3i128 left, int right) => new(left.X <= right, left.Y <= right, left.Z <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(int left, Vec3i128 right) => new(left <= right.X, left <= right.Y, left <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3i128 left, Vec3i128 right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3i128 left, int right) => new(left.X > right, left.Y > right, left.Z > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(int left, Vec3i128 right) => new(left > right.X, left > right.Y, left > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3i128 left, Vec3i128 right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3i128 left, int right) => new(left.X >= right, left.Y >= right, left.Z >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(int left, Vec3i128 right) => new(left >= right.X, left >= right.Y, left >= right.Z);

    public override bool Equals(object? obj) {
        return obj is Vec3i128 other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vec3i128 other) => X == other.X && Y == other.Y && Z == other.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3i(Vec3i128 a) => ((int)a.X, (int)a.Y, (int)a.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3i128(Vec3i a) => new(a.X, a.Y, a.Z);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out Int128 x, out Int128 y, out Int128 z) {
        x = X;
        y = Y;
        z = Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3i128((Int128 X, Int128 Y, Int128 Z) vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}, {Z}>";
}