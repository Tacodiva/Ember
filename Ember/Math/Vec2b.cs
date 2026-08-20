using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Vec2b : IEquatable<Vec2b> {
    public bool X;
    public bool Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2b(bool x, bool y) {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2b((bool X, bool Y) vec) : this(vec.X, vec.Y) { }
    
    public static Vec2b False => default;

    public static Vec2b True => new(true, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator |(Vec2b left, bool right) => new(left.X || right, left.Y || right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator |(Vec2b left, Vec2b right) => new(left.X || right.X, left.Y || right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator &(Vec2b left, bool right) => new(left.X && right, left.Y && right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator &(Vec2b left, Vec2b right) => new(left.X && right.X, left.Y && right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ^(Vec2b left, Vec2b right) => new(left.X ^ right.X, left.Y ^ right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ^(Vec2b left, bool right) => new(left.X ^ right, left.Y ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !(Vec2b v) => new(!v.X, !v.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec2b left, Vec2b right) => left.X == right.X && left.Y == right.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec2b left, Vec2b right) => left.X != right.X || left.Y != right.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(Vec2b left, bool right) => new(left.X == right, left.Y == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(Vec2b left, bool right) => new(left.X != right, left.Y != right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator ==(bool left, Vec2b right) => new(left == right.X, left == right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator !=(bool left, Vec2b right) => new(left != right.X, left != right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2i operator *(Vec2b left, int right) => new(left.X ? right : 0, left.Y ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2f operator *(Vec2b left, float right) => new(left.X ? right : 0, left.Y ? right : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2b operator *(Vec2b left, Vec2b right) => new(left.X && right.X, left.Y && right.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Any() => X || Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool All() => X && Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool None() => !X && !Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Count() {
        int sum = 0;
        if (X) ++sum;
        if (Y) ++sum;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Vec2b otherVector2) return Equals(otherVector2);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vec2b other) {
        return other.X == X && other.Y == Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString() => $"<{X}, {Y}>";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out bool x, out bool y) {
        x = X;
        y = Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (bool X, bool Y)(Vec2b vec) => (vec.X, vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec2b((bool X, bool Y) vec) => new(vec.X, vec.Y);

}
