
using System;
using Ember.Logging;

namespace Ember.Memory;

public static class SpanExt {

    public static unsafe void CopyTo<T>(this Span<T> src, AlignedMemorySpan<T> dst) where T : unmanaged {
        if (src.Length != dst.LengthLong) throw new InvalidOperationException("Spans not the same length.");

        if (dst.Stride == sizeof(T)) {
            src.CopyTo(dst.AsMemorySpan().AsSpan<T>());
        } else {
            for (int i = 0; i < dst.LengthLong; i++)
                dst[i] = src[i];
        }
    }
}