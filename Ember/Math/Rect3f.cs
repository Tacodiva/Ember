
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Rect3f : IEquatable<Rect3f> {

    public static Rect3f UnitCube => new((0, 0, 0), (1, 1, 1));

    public Vec3f PosMin;
    public Vec3f Size;

    public Vec3f PosMax {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return PosMin + Size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Size = value - PosMin;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3f FromCorners(Vec3f posMin, Vec3f posMax) {
        return new(posMin, posMax - posMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3f FromSize(Vec3f posMin, Vec3f size) {
        return new(posMin, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect3f(Vec3f posMin, Vec3f size) {
        PosMin = posMin;
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec3f Midpoint() {
        return PosMin + ((Vec3f)Size / 2f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Area() {
        return Size.X * Size.Y * Size.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ContainsPoint(Vec3f point) {
        return (point >= PosMin).All() && (point <= PosMax).All();
    }

    public override readonly string ToString() => $"Rect3<Pos{PosMin} Size{Size}>";

    public override readonly bool Equals([MaybeNullWhen(false)] object? obj) {
        if (obj is Rect3f rect) return Equals(rect);
        return false;
    }

    public readonly bool Equals(Rect3f other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(PosMin, Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasIntersection(Rect3f rect1, Rect3f rect2) {
        return rect1.PosMin.X < rect2.PosMax.X &&
               rect1.PosMax.X > rect2.PosMin.X &&
               rect1.PosMin.Y < rect2.PosMax.Y &&
               rect1.PosMax.Y > rect2.PosMin.Y &&
               rect1.PosMin.Z < rect2.PosMax.Z &&
               rect1.PosMax.Z > rect2.PosMin.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3f Union(Rect3f rect1, Rect3f rect2) {
        Vec3f min = Vec3f.Min(rect1.PosMin, rect2.PosMin);
        Vec3f max = Vec3f.Max(rect1.PosMax, rect2.PosMax);
        return new(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3f Scale(Rect3f rect, Vec3f scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3f Scale(Rect3f rect, float scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rect3f a, Rect3f b) => (a.PosMin == b.PosMin).All() && (a.Size == b.Size).All();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rect3f a, Rect3f b) => (a.PosMin != b.PosMin).Any() || (a.Size != b.Size).Any();

    public static explicit operator Rect3i(Rect3f rect) => new((Vec3i) rect.PosMin, (Vec3i) rect.Size);
}