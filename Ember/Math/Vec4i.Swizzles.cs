
using System.Runtime.CompilerServices;

namespace Ember.Math;

partial struct Vec4i {
    public Vec2i XY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
        }
    }

    public Vec2i XZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
        }
    }

    public Vec2i XW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
        }
    }

    public Vec2i YX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
        }
    }

    public Vec2i YZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
        }
    }

    public Vec2i YW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
        }
    }

    public Vec2i ZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
        }
    }

    public Vec2i ZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
        }
    }

    public Vec2i ZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
        }
    }

    public Vec2i WX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
        }
    }

    public Vec2i WY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
        }
    }

    public Vec2i WZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
        }
    }


    public Vec3i XYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3i XYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3i XZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i XZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3i XWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i XWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3i YXZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            Z = value.Z;
        }
    }

    public Vec3i YXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3i YZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3i YZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3i YWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3i YWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3i ZYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3i ZYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3i ZXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i ZXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3i ZWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i ZWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3i WYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3i WYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3i WZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i WZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3i WXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3i WXZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
            Z = value.Z;
        }
    }
}