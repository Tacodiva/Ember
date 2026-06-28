
using System.Runtime.CompilerServices;

namespace Ember.Math;

public enum AxialDirection {
    POS_X = 0,
    NEG_X = 1,
    POS_Y = 2,
    NEG_Y = 3,
    POS_Z = 4,
    NEG_Z = 5
}

public static class AxialDirectionExt {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AxialDirection Abs(this AxialDirection f) {
        return (AxialDirection)(((int)f) & ~1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AxialDirection Negate(this AxialDirection f) {
        return (AxialDirection)(((int)f) ^ 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegitive(this AxialDirection f) {
        return (((int)f) & 1) == 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(this AxialDirection f) {
        return (((int)f) & 1) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3i AsVector(this AxialDirection f) {
        return f switch {
            AxialDirection.POS_X => Vec3i.UnitX,
            AxialDirection.POS_Y => Vec3i.UnitY,
            AxialDirection.POS_Z => Vec3i.UnitZ,
            AxialDirection.NEG_X => -Vec3i.UnitX,
            AxialDirection.NEG_Y => -Vec3i.UnitY,
            AxialDirection.NEG_Z => -Vec3i.UnitZ,
            _ => default
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMainAxis(this AxialDirection f) {
        return ((int)f) >> 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int n, int p0, int p1) GetAxies(this AxialDirection f) {
        int n = GetMainAxis(f);
        int p0 = (n + 1) % 3;
        int p1 = (n + 2) % 3;
        return (n, p0, p1);
    }
}