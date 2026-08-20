
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Rect2i : IEquatable<Rect2i> {

    public static Rect2i UnitSquare => new((0, 0), (1, 1));

    public Vec2i PosMin;
    public Vec2i Size;

    public int Width {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Size.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size.X = value;
    }

    public int Height {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Size.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size.Y = value;
    }

    public int X {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => PosMin.X = value;
    }

    public int Y {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => PosMin.Y = value;
    }

    public Vec2i PosMax {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin + Size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size = value - PosMin;
    }

    public Vec2i PosMaxX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (PosMin.X + Size.X, PosMin.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Size.X = value.X - PosMin.X;
            PosMin.Y = value.Y;
        }
    }

    public Vec2i PosMaxY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (PosMin.X, PosMin.Y + Size.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            PosMin.X = value.X;
            Size.Y = value.Y - PosMin.Y;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2i FromCorners(Vec2i posMin, Vec2i posMax) {
        return new(posMin, posMax - posMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2i FromSize(Vec2i posMin, Vec2i size) {
        return new(posMin, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect2i(Vec2i size) {
        PosMin = (0, 0);
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect2i(Vec2i posMin, Vec2i size) {
        PosMin = posMin;
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec2f Midpoint() {
        return PosMin + ((Vec2f)Size / 2f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Area() {
        return Size.X * Size.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ContainsPoint(Vec2i point) {
        return (point >= PosMin).All() && (point <= PosMax).All();
    }

    public override readonly string ToString() => $"Rect2<Pos{PosMin} Size{Size}>";

    public override readonly bool Equals([MaybeNullWhen(false)] object? obj) {
        if (obj is Rect2i rect) return Equals(rect);
        return false;
    }

    public readonly bool Equals(Rect2i other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(PosMin, Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasIntersection(Rect2i rect1, Rect2i rect2) {
        return rect1.PosMin.X < rect2.PosMax.X &&
               rect1.PosMax.X > rect2.PosMin.X &&
               rect1.PosMin.Y < rect2.PosMax.Y &&
               rect1.PosMax.Y > rect2.PosMin.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2i Union(Rect2i rect1, Rect2i rect2) {
        Vec2i min = Vec2i.Min(rect1.PosMin, rect2.PosMin);
        Vec2i max = Vec2i.Max(rect1.PosMax, rect2.PosMax);
        return new(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2i Scale(Rect2i rect, Vec2i scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2i Scale(Rect2i rect, int scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rect2i a, Rect2i b) => a.PosMin == b.PosMin && a.Size == b.Size;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rect2i a, Rect2i b) => a.PosMin != b.PosMin || a.Size != b.Size;

    public static implicit operator Rect2f(Rect2i rect) => new(rect.PosMin, rect.Size);
}