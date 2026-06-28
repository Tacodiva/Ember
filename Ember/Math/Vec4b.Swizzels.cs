using System.Runtime.CompilerServices;

namespace Ember.Math;

partial struct Vec4b {

    public Vec2b XY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
        }
    }

    public Vec2b XZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
        }
    }

    public Vec2b XW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
        }
    }

    public Vec2b YX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
        }
    }

    public Vec2b YZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
        }
    }

    public Vec2b YW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
        }
    }

    public Vec2b ZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
        }
    }

    public Vec2b ZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
        }
    }

    public Vec2b ZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
        }
    }

    public Vec2b WX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
        }
    }

    public Vec2b WY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
        }
    }

    public Vec2b WZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
        }
    }


    public Vec3b XYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3b XYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3b XZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b XZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3b XWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b XWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3b YXZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            Z = value.Z;
        }
    }

    public Vec3b YXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3b YZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3b YZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3b YWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3b YWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3b ZYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3b ZYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3b ZXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b ZXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3b ZWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b ZWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3b WYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3b WYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3b WZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b WZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3b WXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3b WXZ {
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