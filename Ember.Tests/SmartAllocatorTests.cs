
using Ember.Memory;
using static Ember.Memory.SmartAllocator;

namespace Ember.Tests;

public class SmartAllocatorTester {
    public readonly SmartAllocator Allocator;

    private List<Allocation> _allocations;

    public SmartAllocatorTester(ulong size, int alignmentLog2) {
        Allocator = new SmartAllocator(size, alignmentLog2);
        _allocations = new List<Allocation>();
    }

    public void Free(Allocation allocation) {
        Allocator.Free(allocation);
        Assert.True(_allocations.Remove(allocation));
    }

    public void AllocateExpectFail(ulong size, ulong alignment = 0) {
        Assert.False(Allocator.TryAllocate(size, alignment, out _));
    }

    public void AllocateThenFree(ulong size, ulong alignment = 0) {
        Free(Allocate(size, alignment));
    }

    public Allocation Allocate(ulong size, ulong alignment = 0) {
        Assert.True(Allocator.TryAllocate(size, alignment, out Allocation allocation));

        Assert.Equal(allocation.Length, size);

        ulong newAllocationStart = allocation.Offset;
        ulong newAllocationEnd = allocation.Offset + allocation.Length;

        if (alignment < Allocator.Alignment) alignment = Allocator.Alignment;

        if (alignment != 0)
            Assert.True((newAllocationStart & (alignment - 1)) == 0, $"Allocation at 0x{allocation.Offset.ToString("X")} is not aligned to 0x{alignment.ToString("X")}.");

        Assert.InRange(newAllocationStart, 0ul, Allocator.Size);
        Assert.InRange(newAllocationEnd, 0ul, Allocator.Size);

        allocation.CheckDebug(false);

        foreach (Allocation otherAllocation in _allocations) {
            ulong otherAllocationStart = otherAllocation.Offset;
            ulong otherAllocationEnd = otherAllocation.Offset + otherAllocation.Length;

            bool overlaps = (newAllocationStart < otherAllocationEnd) && (newAllocationEnd > otherAllocationStart);
            Assert.False(overlaps, $"New allocation {allocation} overlaps with existing allocation {otherAllocation}");
        }

        _allocations.Add(allocation);

        return allocation;
    }

    public void Print() {
        if (_allocations.Count == 0) Console.WriteLine("Allocator empty.");
        else _allocations[0].CheckDebug(true);
    }

}
public class SmartAllocatorTests {

    [Fact]
    public void Simple() {
        SmartAllocatorTester allocator = new SmartAllocatorTester(16, 0);

        allocator.Free(allocator.Allocate(10));
        allocator.Free(allocator.Allocate(12));
        allocator.Free(allocator.Allocate(16));

        allocator.Allocate(1);
        allocator.Allocate(1);
        allocator.Allocate(14);
    }

    [Fact]
    public void FailAllocations() {
        SmartAllocatorTester allocator = new SmartAllocatorTester(16, 0);

        allocator.AllocateExpectFail(20);

        Allocation allocation = allocator.Allocate(1);
        allocator.AllocateExpectFail(16);

        allocator.Free(allocation);
        allocator.Allocate(16);
    }

    [Fact]
    public void Alignment() {
        SmartAllocatorTester allocator = new SmartAllocatorTester(16, 1);

        allocator.Allocate(1);
        allocator.Allocate(1);
        allocator.Allocate(4, 8);
        allocator.AllocateExpectFail(1, 8);
    }

    [Fact]
    public void Alignment2() {
        SmartAllocatorTester allocator = new SmartAllocatorTester(16, 1);

        allocator.AllocateThenFree(1, 16);

        Allocation a1 = allocator.Allocate(1, 1 << 10);
        allocator.AllocateExpectFail(1, 16);
        Allocation a2 = allocator.Allocate(8, 8);
        allocator.AllocateExpectFail(1, 8);

        allocator.Free(a1);
        a1 = allocator.Allocate(8, 8);

        allocator.Free(a1);
        allocator.Free(a2);

        allocator.Allocate(16, 16);
    }

    [Fact]
    public void Alignment3() {
        SmartAllocatorTester allocator = new SmartAllocatorTester(20, 1);

        Allocation a1 = allocator.Allocate(1, 16);
        Allocation a2 = allocator.Allocate(8, 8);
        allocator.Free(a1);

        allocator.AllocateThenFree(1, 0);
        allocator.AllocateThenFree(1, 1 << 10);
        allocator.AllocateExpectFail(11, 0);
        allocator.AllocateExpectFail(11, 16);
        allocator.AllocateExpectFail(11, 1 << 10);

        allocator.Allocate(8, 16);
        allocator.Allocate(4, 16);
        allocator.Free(a2);

        allocator.AllocateThenFree(1, 0);
        allocator.AllocateThenFree(7, 8);
        allocator.AllocateThenFree(8, 8);
        allocator.AllocateExpectFail(9, 0);
        allocator.AllocateExpectFail(7, 16);
        allocator.AllocateExpectFail(7, 1 << 10);
    }


}