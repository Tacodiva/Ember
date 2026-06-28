
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Rect3i : IEquatable<Rect3i> {

    public static Rect3i UnitCube => new((0, 0, 0), (1, 1, 1));

    public Vec3i PosMin;
    public Vec3i Size;

    public Vec3i PosMax {
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
    public static Rect3i FromCorners(Vec3i posMin, Vec3i posMax) {
        return new(posMin, posMax - posMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3i FromSize(Vec3i posMin, Vec3i size) {
        return new(posMin, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect3i(Vec3i posMin, Vec3i size) {
        PosMin = posMin;
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec3f Midpoint() {
        return PosMin + ((Vec3f)Size / 2f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Area() {
        return Size.X * Size.Y * Size.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ContainsPoint(Vec3i point) {
        return (point >= PosMin).All() && (point <= PosMax).All();
    }

    public override readonly string ToString() => $"Rect3<Pos{PosMin} Size{Size}>";

    public override readonly bool Equals([MaybeNullWhen(false)] object? obj) {
        if (obj is Rect3i rect) return Equals(rect);
        return false;
    }

    public readonly bool Equals(Rect3i other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(PosMin, Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasIntersection(Rect3i rect1, Rect3i rect2) {
        return rect1.PosMin.X < rect2.PosMax.X &&
               rect1.PosMax.X > rect2.PosMin.X &&
               rect1.PosMin.Y < rect2.PosMax.Y &&
               rect1.PosMax.Y > rect2.PosMin.Y &&
               rect1.PosMin.Z < rect2.PosMax.Z &&
               rect1.PosMax.Z > rect2.PosMin.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3i Union(Rect3i rect1, Rect3i rect2) {
        Vec3i min = Vec3i.Min(rect1.PosMin, rect2.PosMin);
        Vec3i max = Vec3i.Max(rect1.PosMax, rect2.PosMax);
        return new(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3i Scale(Rect3i rect, Vec3i scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect3i Scale(Rect3i rect, int scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rect3i a, Rect3i b) => (a.PosMin == b.PosMin).All() && (a.Size == b.Size).All();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rect3i a, Rect3i b) => (a.PosMin != b.PosMin).Any() || (a.Size != b.Size).Any();

    public static implicit operator Rect3f(Rect3i rect) => new(rect.PosMin, rect.Size);
}