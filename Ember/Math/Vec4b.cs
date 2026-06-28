using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Ember.Math;

public partial struct Vec4b : IEquatable<Vec4b> {
    public bool X;
    public bool Y;
    public bool Z;
    public bool W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4b(bool x, bool y, bool z, bool w) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4b((bool x, bool y, bool z, bool w) vec) : this(vec.x, vec.y, vec.z, vec.w) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4b(Vec3b xyz, bool w) : this(xyz.X, xyz.Y, xyz.Z, w) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4b(bool x, Vec3b yzw) : this(x, yzw.X, yzw.Y, yzw.Y) { }

    public static Vec4b False => default;

    public static Vec4b True => new(true, true, true, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator |(Vec4b left, bool right) => new(left.X || right, left.Y || right, left.Z || right, left.W || right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator |(Vec4b left, Vec4b right) => new(left.X || right.X, left.Y || right.Y, left.Z || right.Z, left.W || right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator &(Vec4b left, bool right) => new(left.X && right, left.Y && right, left.Z && right, left.W && right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator &(Vec4b left, Vec4b right) => new(left.X && right.X, left.Y && right.Y, left.Z && right.Z, left.W && right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ^(Vec4b left, Vec4b right) => new(left.X ^ right.X, left.Y ^ right.Y, left.Z ^ right.Z, left.W ^ right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ^(Vec4b left, bool right) => new(left.X ^ right, left.Y ^ right, left.Z ^ right, left.W ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !(Vec4b v) => new(!v.X, !v.Y, !v.Z, !v.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(Vec4b left, int right) => new(left.X ? right : 0, left.Y ? right : 0, left.Z ? right : 0, left.W ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f operator *(Vec4b left, float right) => new(left.X ? right : 0, left.Y ? right : 0, left.Z ? right : 0, left.W ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(Vec4b left, bool right) => new(left.X == right, left.Y == right, left.Z == right, left.W == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(Vec4b left, bool right) => new(left.X != right, left.Y != right, left.Z != right, left.W != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator ==(bool left, Vec4b right) => new(left == right.X, left == right.Y, left == right.Z, left == right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4b operator !=(bool left, Vec4b right) => new(left != right.X, left != right.Y, left != right.Z, left != right.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec4b left, Vec4b right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec4b left, Vec4b right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z || left.W != right.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Any() => X || Y || Z || W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool All() => X && Y && Z && W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool None() => !X && !Y && !Z && !W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Count() {
        int sum = 0;
        if (X) ++sum;
        if (Y) ++sum;
        if (Z) ++sum;
        if (W) ++sum;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec4b otherVector4) return Equals(otherVector4);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec4b other) {
        return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}, {Z}, {W}>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out bool x, out bool y, out bool z, out bool w) {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (bool X, bool Y, bool Z, bool W)(Vec4b vec) => (vec.X, vec.Y, vec.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4b((bool X, bool Y, bool Z, bool W) vec) => new(vec.X, vec.Y, vec.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4b((Vec3b XYZ, bool W) vec) => new(vec.XYZ.X, vec.XYZ.Y, vec.XYZ.Z, vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4b((bool X, Vec3b YZW) vec) => new(vec.X, vec.YZW.X, vec.YZW.Y, vec.YZW.Z);

}