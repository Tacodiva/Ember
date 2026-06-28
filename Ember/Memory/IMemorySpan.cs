
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ember.Memory;

public interface IMemorySpan {
    public MemorySpan AsMemorySpan();
}

public interface IMemorySpan<T> : IEnumerable<T> where T : unmanaged {
    public MemorySpan<T> AsMemorySpan();
}

public static class IMemorySpanExt {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemorySpan AsSpan(this IMemorySpan span, int offsetBytes) {
        return span.AsMemorySpan().Slice(offsetBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AlignedMemorySpan<T> AsAlignedSpan<T>(this IMemorySpan span, int alignment) where T : unmanaged {
        return span.AsMemorySpan().AsAlignedSpan<T>(alignment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this IMemorySpan span, int offsetBytes = 0) where T : unmanaged {
        return span.AsMemorySpan().AsSpan<T>(offsetBytes);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this IMemorySpan span, int offsetBytes, int lengthBytes) where T : unmanaged {
        return span.AsMemorySpan().AsSpan<T>(offsetBytes, lengthBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Ptr<T> AsPointer<T>(this IMemorySpan span) where T : unmanaged {
        return span.AsMemorySpan().AsPointer<T>();
    }
    
}