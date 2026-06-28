
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Vec3b : IEquatable<Vec3b> {

    public bool X;
    public bool Y;
    public bool Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3b(bool x, bool y, bool z) {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3b((bool x, bool y, bool z) vec) : this(vec.x, vec.y, vec.z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3b(Vec2b xy, bool z) : this(xy.X, xy.Y, z) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3b(bool x, Vec2b yz) : this(x, yz.X, yz.Y) { }

    public static Vec3b False => default;

    public static Vec3b True => new(true, true, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator |(Vec3b left, bool right) => new(left.X || right, left.Y || right, left.Z || right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator |(Vec3b left, Vec3b right) => new(left.X || right.X, left.Y || right.Y, left.Z || right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator &(Vec3b left, bool right) => new(left.X && right, left.Y && right, left.Z && right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator &(Vec3b left, Vec3b right) => new(left.X && right.X, left.Y && right.Y, left.Z && right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ^(Vec3b left, Vec3b right) => new(left.X ^ right.X, left.Y ^ right.Y, left.Z ^ right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ^(Vec3b left, bool right) => new(left.X ^ right, left.Y ^ right, left.Z ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i operator *(Vec3b left, int right) => new(left.X ? right : 0, left.Y ? right : 0, left.Z ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(Vec3b left, float right) => new(left.X ? right : 0, left.Y ? right : 0, left.Z ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !(Vec3b v) => new(!v.X, !v.Y, !v.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(Vec3b left, bool right) => new(left.X == right, left.Y == right, left.Z == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(Vec3b left, bool right) => new(left.X != right, left.Y != right, left.Z != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator ==(bool left, Vec3b right) => new(left == right.X, left == right.Y, left == right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3b operator !=(bool left, Vec3b right) => new(left != right.X, left != right.Y, left != right.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec3b left, Vec3b right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec3b left, Vec3b right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Any() => X || Y || Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool All() => X && Y && Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool None() => !X && !Y && !Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Count() {
        int sum = 0;
        if (X) ++sum;
        if (Y) ++sum;
        if (Z) ++sum;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec3b otherVector2) return Equals(otherVector2);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec3b other) {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}, {Z}>";


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out bool x, out bool y, out bool z) {
        x = X;
        y = Y;
        z = Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (bool X, bool Y, bool Z)(Vec3b vec) => (vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3b((bool X, bool Y, bool Z) vec) => new(vec.X, vec.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3b((Vec2b XY, bool Z) vec) => new(vec.XY.X, vec.XY.Y, vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3b((bool X, Vec2b YZ) vec) => new(vec.X, vec.YZ.X, vec.YZ.Y);

    #region Swizzels
    public Vec2b XY {
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

    public Vec2b XZ {
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

    public Vec2b YX {
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

    public Vec2b YZ {
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

    public Vec2b ZX {
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

    public Vec2b ZY {
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