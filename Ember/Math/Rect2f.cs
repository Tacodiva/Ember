
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public struct Rect2f : IEquatable<Rect2f> {

    public static Rect2f UnitSquare => new((0, 0), (1, 1));

    public Vec2f PosMin;
    public Vec2f Size;

    public float Width {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Size.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size.X = value;
    }

    public float Height {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Size.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size.Y = value;
    }

    public float X {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => PosMin.X = value;
    }

    public float Y {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => PosMin.Y = value;
    }

    public Vec2f PosMax {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => PosMin + Size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Size = value - PosMin;
    }

    public Vec2f PosMaxX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (PosMin.X + Size.X, PosMin.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Size.X = value.X - PosMin.X;
            PosMin.Y = value.Y;
        }
    }

    public Vec2f PosMaxY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (PosMin.X, PosMin.Y + Size.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            PosMin.X = value.X;
            Size.Y = value.Y - PosMin.Y;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2f FromCorners(Vec2f posMin, Vec2f posMax) {
        return new(posMin, posMax - posMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2f FromSize(Vec2f posMin, Vec2f size) {
        return new(posMin, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect2f(Vec2f posMin, Vec2f size) {
        PosMin = posMin;
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec2f Midpofloat() {
        return PosMin + (Size / 2f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Area() {
        return Size.X * Size.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ContainsPoint(Vec2f point) {
        return (point >= PosMin).All() && (point <= PosMax).All();
    }

    public override readonly string ToString() => $"Rect2<Pos{PosMin} Size{Size}>";

    public override readonly bool Equals([MaybeNullWhen(false)] object? obj) {
        if (obj is Rect2f rect) return Equals(rect);
        return false;
    }

    public readonly bool Equals(Rect2f other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(PosMin, Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Hasfloatersection(Rect2f rect1, Rect2f rect2) {
        return rect1.PosMin.X < rect2.PosMax.X &&
               rect1.PosMax.X > rect2.PosMin.X &&
               rect1.PosMin.Y < rect2.PosMax.Y &&
               rect1.PosMax.Y > rect2.PosMin.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2f Union(Rect2f rect1, Rect2f rect2) {
        Vec2f min = Vec2f.Min(rect1.PosMin, rect2.PosMin);
        Vec2f max = Vec2f.Max(rect1.PosMax, rect2.PosMax);
        return new(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2f Scale(Rect2f rect, Vec2f scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect2f Scale(Rect2f rect, float scale) {
        rect.PosMin *= scale;
        rect.Size *= scale;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rect2f a, Rect2f b) => a.PosMin == b.PosMin && a.Size == b.Size;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rect2f a, Rect2f b) => a.PosMin != b.PosMin || a.Size != b.Size;

    public static explicit operator Rect2i(Rect2f rect) => new((Vec2i)rect.PosMin, (Vec2i)rect.Size);
}