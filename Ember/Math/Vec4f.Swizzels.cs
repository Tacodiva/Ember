using System.Runtime.CompilerServices;

namespace Ember.Math;

partial struct Vec4f {

    public Vec2f XY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
        }
    }

    public Vec2f XZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
        }
    }

    public Vec2f XW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
        }
    }

    public Vec2f YX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
        }
    }

    public Vec2f YZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
        }
    }

    public Vec2f YW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
        }
    }

    public Vec2f ZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
        }
    }

    public Vec2f ZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
        }
    }

    public Vec2f ZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
        }
    }

    public Vec2f WX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
        }
    }

    public Vec2f WY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
        }
    }

    public Vec2f WZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
        }
    }


    public Vec3f XYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3f XYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3f XZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f XZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3f XWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f XWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(X, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            X = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3f YXZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            Z = value.Z;
        }
    }

    public Vec3f YXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3f YZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3f YZW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, Z, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            Z = value.Y;
            W = value.Z;
        }
    }

    public Vec3f YWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3f YWZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Y, W, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Y = value.X;
            W = value.Y;
            Z = value.Z;
        }
    }

    public Vec3f ZYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3f ZYW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, Y, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            Y = value.Y;
            W = value.Z;
        }
    }

    public Vec3f ZXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f ZXW {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, X, W);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            X = value.Y;
            W = value.Z;
        }
    }

    public Vec3f ZWY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f ZWX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(Z, W, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            Z = value.X;
            W = value.Y;
            X = value.Z;
        }
    }

    public Vec3f WYZ {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public Vec3f WYX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Y, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Y = value.Y;
            X = value.Z;
        }
    }

    public Vec3f WZY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f WZX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, Z, X);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            Z = value.Y;
            X = value.Z;
        }
    }

    public Vec3f WXY {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get {
            return new(W, X, Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set {
            W = value.X;
            X = value.Y;
            Y = value.Z;
        }
    }

    public Vec3f WXZ {
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