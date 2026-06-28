
using System;
using Ember.Memory;

namespace Ember.Tests;

public struct BadHashValue(ulong value) : IComparable<BadHashValue> {
    public ulong Value = value;

    public override readonly int GetHashCode() => 0;

    public readonly bool Equals(BadHashValue other) => Value == other.Value;
    public override readonly bool Equals(object? obj) => obj is BadHashValue other && Equals(other);

    public readonly int CompareTo(BadHashValue other) => Value.CompareTo(other.Value);

    public static bool operator ==(BadHashValue a, BadHashValue b) => a.Equals(b);
    public static bool operator !=(BadHashValue a, BadHashValue b) => !a.Equals(b);

    public static implicit operator BadHashValue(ulong value) => new(value);
}

public static class TestUtils {

    public static void CheckMemoryFreed(Action action) {
        Assert.True(MemoryUtils.TrackAllocationTraces);
        MemoryUtils.TrackAllocationTraces = true;

        int startingAllocations = MemoryUtils.AllocationCount;

        action();

        int endingAllocations = MemoryUtils.AllocationCount;

        if (startingAllocations != endingAllocations) {
            Assert.Fail($"{endingAllocations - startingAllocations} allocations not freed.\n{MemoryUtils.DumpAllocations()}");
        }
    }
}