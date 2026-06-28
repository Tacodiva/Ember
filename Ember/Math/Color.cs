
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ember.Math;

public partial struct Color : IEquatable<Color> {

    public static Color FromHSV(float hue, float saturation, float value) {
        int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
        float f = hue / 60 - MathF.Floor(hue / 60);

        value *= 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return FromRGB(v, t, p);
        else if (hi == 1)
            return FromRGB(q, v, p);
        else if (hi == 2)
            return FromRGB(p, v, t);
        else if (hi == 3)
            return FromRGB(p, q, v);
        else if (hi == 4)
            return FromRGB(t, p, v);
        else
            return FromRGB(v, p, q);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromVector4(Vec4f vector) => new(vector);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromVector3(Vec3f vector) => new(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromRGBA(uint rgba) => new(rgba);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromRGBA(int r, int g, int b, int a) => new(r, g, b, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromRGBA(byte r, byte g, byte b, byte a) => new(r, g, b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromRGB(int r, int g, int b) => new(r, g, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromRGB(byte r, byte g, byte b) => new(r, g, b);

    public uint RGBA;

    private const int RedOffset = 24;
    public byte R {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return (byte)((RGBA >> RedOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            RGBA = (uint)((RGBA & ~(0xFF << RedOffset)) | (uint)value << RedOffset);
        }
    }

    private const int GreenOffset = 16;
    public byte G {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return (byte)((RGBA >> GreenOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            RGBA = (uint)((RGBA & ~(0xFF << GreenOffset)) | (uint)value << GreenOffset);
        }
    }

    private const int BlueOffset = 8;
    public byte B {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return (byte)((RGBA >> BlueOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            RGBA = (uint)((RGBA & ~(0xFF << BlueOffset)) | (uint)value << BlueOffset);
        }
    }

    private const int AlphaOffset = 0;
    public byte A {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get {
            return (byte)((RGBA >> AlphaOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            RGBA = (uint)((RGBA & ~(0xFF << AlphaOffset)) | (uint)value << AlphaOffset);
        }
    }

    public float Hue {
        readonly get {
            (float h, _, _) = ToHSV();
            return h;
        }
        set {
            (_, float s, float v) = ToHSV();
            this = FromHSV(value, s, v);
        }
    }

    public float Saturation {
        readonly get {
            int red = R, green = G, blue = B;
            float value;

            float min = System.Math.Min(System.Math.Min(red, green), blue);
            value = System.Math.Max(System.Math.Max(red, green), blue);
            float delta = value - min;

            if (value == 0) return 0;
            return delta / value;
        }
        set {
            (float h, _, float v) = ToHSV();
            this = FromHSV(h, value, v);
        }
    }

    public float Value {
        readonly get {
            return System.Math.Min(System.Math.Min(R, G), B) / 255f;
        }
        set {
            (float h, float s, _) = ToHSV();
            this = FromHSV(h, s, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(uint rgb) {
        RGBA = rgb;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(byte r, byte g, byte b, byte a) {
        RGBA = (uint)(
            r << RedOffset |
            g << GreenOffset |
            b << BlueOffset |
            a << AlphaOffset
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(int r, int g, int b, int a) : this(
        (byte) System.Math.Clamp(r, 0, 0xFF),
        (byte) System.Math.Clamp(g, 0, 0xFF),
        (byte) System.Math.Clamp(b, 0, 0xFF),
        (byte) System.Math.Clamp(a, 0, 0xFF)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(Vec4i vector) : this(vector.X, vector.Y, vector.Z, vector.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(Vec3i vector) : this(new Vec4i(vector, 0xFF)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(Vec4f vector) : this((Vec4i)(vector * 0xFF)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(Vec3f vector) : this((vector, 1)) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(byte r, byte g, byte b) : this((int)r, (int)g, (int)b) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(int r, int g, int b) : this(r, g, b, 0xFF) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec3i ToVec3i() {
        return (R, G, B);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec3f ToVec3f() {
        return (R / 255f, G / 255f, B / 255f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec4f ToVec4f() {
        return (R / 255f, G / 255f, B / 255f, A / 255f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vec4i ToVec4i() {
        return (R, G, B, A);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out int r, out int g, out int b, out int a) {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float r, out float g, out float b, out float a) {
        r = R / 255f;
        g = G / 255f;
        b = B / 255f;
        a = A / 255f;
    }

    public readonly (float H, float S, float V) ToHSV() {
        int red = R, green = G, blue = B;

        float min = System.Math.Min(System.Math.Min(red, green), blue);
        float v = System.Math.Max(System.Math.Max(red, green), blue);
        float delta = v - min;

        float s;
        if (v == 0.0)
            s = 0;
        else
            s = delta / v;

        float h = 0;
        if (s != 0) {
            if (red == v)
                h = (green - blue) / delta;
            else if (green == v)
                h = 2 + (blue - red) / delta;
            else if (blue == v)
                h = 4 + (red - green) / delta;

            h *= 60;

            if (h < 0.0)
                h += 360;
        }

        return (h, s, (v / 255f));
    }

    public override readonly string ToString() {
        if (A == 255) return "0x" + (RGBA >> 8).ToString("X6");
        else return "0x" + RGBA.ToString("X8");
    }

    public override readonly int GetHashCode() => (int) RGBA;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Color color) return Equals(color);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Color color) {
        return color.RGBA == RGBA;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Color a, Color b) => a.RGBA == b.RGBA;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Color a, Color b) => a.RGBA != b.RGBA;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f(Color color) => color.ToVec3f();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec4f(Color color) => color.ToVec4f();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color((int R, int G, int B) rgb) => new(rgb.R, rgb.G, rgb.B);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color((int R, int G, int B, int A) rgba) => new(rgba.R, rgba.G, rgba.B, rgba.A);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color((float R, float G, float B) rgb) => new(new Vec3f(rgb.R, rgb.G, rgb.B));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color((float R, float G, float B, float A) rgba) => new(new Vec4f(rgba.R, rgba.G, rgba.B, rgba.A));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(Vec3f vector) => new(vector);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(Vec4f vector) => new(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(Vec3i vector) => new(vector);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(Vec4i vector) => new(vector);
}