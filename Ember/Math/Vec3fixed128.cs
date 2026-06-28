
using System;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Vec3fixed128 {

    public Fixed128 X, Y, Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3fixed128(Fixed128 x, Fixed128 y, Fixed128 z) {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator +(Vec3fixed128 left, Vec3fixed128 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator +(Vec3fixed128 left, Vec3i right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator +(Vec3fixed128 left, Vec3f right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator +(Vec3i left, Vec3fixed128 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator +(Vec3f left, Vec3fixed128 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator -(Vec3fixed128 left, Vec3fixed128 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator -(Vec3fixed128 left, Vec3i right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator -(Vec3fixed128 left, Vec3f right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator -(Vec3i left, Vec3fixed128 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3fixed128 operator -(Vec3f left, Vec3fixed128 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3fixed128 left, Fixed128 right) => new(left.X == right, left.Y == right, left.Z == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3fixed128 left, Fixed128 right) => new(left.X != right, left.Y != right, left.Z != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Fixed128 left, Vec3fixed128 right) => new(left == right.X, left == right.Y, left == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Fixed128 left, Vec3fixed128 right) => new(left != right.X, left != right.Y, left != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3fixed128 left, Vec3fixed128 right) => new(left.X == right.X, left.Y == right.Y, left.Z == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3fixed128 left, Vec3fixed128 right) => new(left.X != right.X, left.Y != right.Y, left.Z != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3fixed128 left, Vec3fixed128 right) => new(left.X < right.X, left.Y < right.Y, left.Z < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Vec3fixed128 left, Fixed128 right) => new(left.X < right, left.Y < right, left.Z < right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <(Fixed128 left, Vec3fixed128 right) => new(left < right.X, left < right.Y, left < right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3fixed128 left, Vec3fixed128 right) => new(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Vec3fixed128 left, Fixed128 right) => new(left.X <= right, left.Y <= right, left.Z <= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator <=(Fixed128 left, Vec3fixed128 right) => new(left <= right.X, left <= right.Y, left <= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3fixed128 left, Vec3fixed128 right) => new(left.X > right.X, left.Y > right.Y, left.Z > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Vec3fixed128 left, Fixed128 right) => new(left.X > right, left.Y > right, left.Z > right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >(Fixed128 left, Vec3fixed128 right) => new(left > right.X, left > right.Y, left > right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3fixed128 left, Vec3fixed128 right) => new(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Vec3fixed128 left, Fixed128 right) => new(left.X >= right, left.Y >= right, left.Z >= right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator >=(Fixed128 left, Vec3fixed128 right) => new(left >= right.X, left >= right.Y, left >= right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out Fixed128 x, out Fixed128 y, out Fixed128 z) {
        x = X;
        y = Y;
        z = Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3fixed128((Fixed128 X, Fixed128 Y, Fixed128 Z) vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3i128(in Vec3fixed128 value) => ((Int128)value.X, (Int128)value.Y, (Int128)value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3fixed128(in Vec3i128 value) => ((Fixed128)value.X, (Fixed128)value.Y, (Fixed128)value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3f(in Vec3fixed128 value) => ((float)value.X, (float)value.Y, (float)value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3i(in Vec3fixed128 value) => ((int)value.X, (int)value.Y, (int)value.Z);

    public readonly override bool Equals(object? obj) => obj is Vec3fixed128 vec && Equals(vec);

    public readonly bool Equals(Vec3fixed128 obj) => (this == obj).Any();

    public readonly override int GetHashCode() => HashCode.Combine(X, Y, Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}f128, {Y}f128, {Z}f128>";

    public static Vec3fixed128 Round(Vec3fixed128 value) => (Fixed128.Round(value.X), Fixed128.Round(value.Y), Fixed128.Round(value.Z));
    public static Vec3fixed128 Floor(Vec3fixed128 value) => (Fixed128.Floor(value.X), Fixed128.Floor(value.Y), Fixed128.Floor(value.Z));
    public static Vec3fixed128 Ceiling(Vec3fixed128 value) => (Fixed128.Ceiling(value.X), Fixed128.Ceiling(value.Y), Fixed128.Ceiling(value.Z));
}