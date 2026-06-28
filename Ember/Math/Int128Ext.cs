
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ember.Math;

public static class Int128Ext {

    // https://github.com/dotnet/runtime/blob/fc463550ed7f23e3fa30962292446793526e6d8a/src/libraries/System.Private.CoreLib/src/System/Int128.cs
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct TransparentInt128 {
#if BIGENDIAN
        public readonly ulong Upper;
        public readonly ulong Lower;
#else
        public readonly ulong Lower;
        public readonly ulong Upper;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetUpper(this Int128 int128) => Unsafe.As<Int128, TransparentInt128>(ref int128).Upper;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetLower(this Int128 int128) => Unsafe.As<Int128, TransparentInt128>(ref int128).Lower;
}