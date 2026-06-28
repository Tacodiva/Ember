
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Ember.Collections;

namespace Ember.Memory;

public sealed class SmartAllocator {

    public readonly int AlignmentLog2;
    public readonly ulong Alignment;
    public readonly ulong AlignmentMask;

    public readonly ulong Size;

    private int _allocationCount;
    private ulong _allocationSize;

    public ulong AllocationSize => _allocationSize;
    public ulong FreeSpace => Size - AllocationSize;
    public int AllocationCount => _allocationCount;


    // A list of free regions, sorted by their size.
    private readonly List<Region> _freeList;
    private readonly ConcurrentObjectPool<Region> _regionPool;
    private readonly Lock _lock;

    public SmartAllocator(ulong size, int alignmentLog2 = 0) {
        Size = size;
        _lock = new();
        _freeList = new();
        _regionPool = new(() => new Region(this));

        AlignmentLog2 = alignmentLog2;
        Alignment = 1ul << AlignmentLog2;
        AlignmentMask = ~(~0ul << AlignmentLog2);

        _allocationCount = 0;
        _allocationSize = 0;

        _freeList.Add(_regionPool.Borrow().Init(0, Size, 0, null, null));
    }

    /// <summary>
    /// Finds the first free region with at least a given size.
    /// </summary>
    /// <returns>The index of the region or the length of the array if all regions are too small.</returns>
    private int SearchRegions(ulong size) {
        // A simple binary search
        int lo = 0;
        int high = _freeList.Count - 1;
        while (lo <= high) {
            int mid = (high - lo) / 2 + lo;
            ulong midSize = _freeList[mid].Size;
            if (midSize > size) {
                high = mid - 1;
            } else if (midSize == size) {
                high = mid - 1;
            } else
                lo = mid + 1;
        }
        return lo;
    }

    private Region AddFreeRegion(Region region) {
        int index = SearchRegions(region.Size);
        region.FreeIndex = index;
        _freeList.Insert(index, region);
#if EMBER_SAFETY_CHECKS
        ++region.Version;
#endif
        for (int i = index + 1; i < _freeList.Count; i++)
            _freeList[i].FreeIndex = i;
        return region;
    }

    private Region RemoveFreeRegion(Region region) {
        int index = region.FreeIndex;
        _freeList.RemoveAt(index);
        region.FreeIndex = -1;
#if EMBER_SAFETY_CHECKS
        ++region.Version;
#endif
        for (int i = index; i < _freeList.Count; i++)
            _freeList[i].FreeIndex = i;
        return region;
    }

    public bool IsEmpty {
        get {
            lock (_lock)
                return _freeList.Count == 1 && _freeList[0].IsFree && _freeList[0].Size == Size && _freeList[0].Offset == 0;
        }
    }

    public bool TryAllocate(int size, ulong alignment, [MaybeNullWhen(false)] out Allocation allocation) {
        checked {
            return TryAllocate((ulong)size, alignment, out allocation);
        }
    }

    public bool TryAllocate(ulong size, ulong alignment, [MaybeNullWhen(false)] out Allocation allocation) {
        if ((alignment & (alignment - 1)) != 0)
            throw new ArgumentException($"Expected {nameof(alignment)} to be a power of 2. Got {alignment}.");

        if (size == 0) {
            allocation = default;
            return true;
        }

        if (size > FreeSpace) {
            allocation = default;
            return false;
        }

        lock (_lock) {

            int regionIndex = SearchRegions(size);
            if (regionIndex == _freeList.Count) {
                allocation = default;
                return false;
            }

            Region region = _freeList[regionIndex];
            // The offset of the allocation within the region
            ulong allocOffset = 0;

            if (alignment > Alignment) {
                ulong alignmentMask = alignment - 1;

                while (regionIndex < _freeList.Count) {
                    region = _freeList[regionIndex];

                    if ((region.Offset & alignmentMask) == 0) {
                        allocOffset = 0;
                        break;
                    }
                    ulong alignedOffset = (region.Offset & ~alignmentMask) + alignment;
                    allocOffset = alignedOffset - region.Offset;

                    if (region.Size > allocOffset && region.Size - allocOffset >= size)
                        break;

                    ++regionIndex;
                }
                if (regionIndex == _freeList.Count) {
                    allocation = default;
                    return false;
                }
            }

            // Align the size
            ulong regionSize = size;
            if ((regionSize & AlignmentMask) != 0) {
                regionSize &= ~AlignmentMask;
                regionSize += Alignment;
            }

            RemoveFreeRegion(region);

            if (allocOffset != 0) {
                if (region.Previous != null && region.Previous.IsFree) {
                    region.Previous.Size += allocOffset;
                } else {
                    Region allocRegionExtra = _regionPool.Borrow().Init(region.Offset, allocOffset, -1, region, region.Previous);
                    if (region.Previous != null)
                        region.Previous.Next = allocRegionExtra;
                    region.Previous = allocRegionExtra;
                    AddFreeRegion(allocRegionExtra);
                }
                region.Offset += allocOffset;
                region.Size -= allocOffset;
            }


            // The aligned space after the allocation
            ulong extraSpaceAfter = region.Size - regionSize;

            if (extraSpaceAfter != 0) {
                if (region.Next != null && region.Next.IsFree) {
                    region.Next.Offset -= extraSpaceAfter;
                    region.Next.Size += extraSpaceAfter;
                } else {
                    Region allocRegionExtra = _regionPool.Borrow().Init(region.Offset + regionSize, extraSpaceAfter, -1, region.Next, region);
                    if (region.Next != null)
                        region.Next.Previous = allocRegionExtra;
                    AddFreeRegion(allocRegionExtra);
                    region.Next = allocRegionExtra;
                }
                region.Size = regionSize;
            }

            allocation = new(region, size);

            _allocationSize += size;
            ++_allocationCount;

            return true;
        }
    }

    public void Free(in Allocation allocation) {
        if (allocation.IsNull) return;
        lock (_lock) {
#if EMBER_SAFETY_CHECKS
            allocation.CheckVersion();
#endif
            ulong length = allocation.Length;
            Region region = allocation.Region;

            Region? previous = region.Previous;
            if (previous != null && previous.IsFree) {
                RemoveFreeRegion(previous);
                region.Offset = previous.Offset;
                region.Size += previous.Size;
                region.Previous = previous.Previous;
                if (region.Previous != null)
                    region.Previous.Next = region;
                _regionPool.Return(previous);
            }

            Region? next = region.Next;
            if (next != null && next.IsFree) {
                RemoveFreeRegion(next);
                region.Size += next.Size;
                region.Next = next.Next;
                if (region.Next != null)
                    region.Next.Previous = region;
                _regionPool.Return(next);
            }

            AddFreeRegion(region);

            _allocationSize -= length;
            --_allocationCount;
        }
    }

    public struct Allocation {
        internal readonly Region Region;
        public readonly ulong Length;
        public ulong Offset => Region?.Offset ?? 0;
        public bool IsNull => Region == null;

#if EMBER_SAFETY_CHECKS
        public uint RegionVersion;
#endif

        internal Allocation(Region region, ulong length) {
            Region = region;
            Length = length;
#if EMBER_SAFETY_CHECKS
            RegionVersion = region.Version;
#endif
        }

#if EMBER_SAFETY_CHECKS
        public void CheckVersion() {
            if (Region.Version != RegionVersion)
                throw new ArgumentException("Region version mismatch. This could indicate a double free.");
        }
#endif

        [Conditional("DEBUG")]
        public void CheckDebug(bool print) {
            Region.CheckDebug(print);
        }

        public override string ToString() {
            if (IsNull) return $"NullAllocation[0x{Offset.ToString("X")} + ???]";
            return $"Allocation[0x{Offset.ToString("X")} + 0x{Length.ToString("X")}]";
        }

    }

    /// <summary>
    /// A region of memory. It may be allocated or free.
    /// </summary>
    internal class Region {
        public readonly SmartAllocator Allocator;

        public ulong Offset;
        public ulong Size;

#if EMBER_SAFETY_CHECKS
        public uint Version;
#endif

        /// <summary>
        /// The index of this region in the free list or -1 if allocated;
        /// </summary>
        public int FreeIndex;

        public bool IsAllocated => FreeIndex == -1;
        public bool IsFree => FreeIndex != -1;

        /// <summary>
        /// The region directly following this one in memory, or null if it's the last region.
        /// </summary>
        public Region? Next;
        /// <summary>
        /// The region directly before this one in memory, or null if it's the first region.
        /// </summary>
        public Region? Previous;

        public Region(SmartAllocator allocator) {
            Allocator = allocator;
            FreeIndex = -1;
        }

        public Region Init(ulong offset, ulong size, int freeIndex, Region? next, Region? previous) {
            Offset = offset;
            Size = size;
            Next = next;
            Previous = previous;
            FreeIndex = freeIndex;
#if EMBER_SAFETY_CHECKS
            ++Version;
#endif
            return this;
        }

        [Conditional("DEBUG")]
        public void CheckDebug(bool print) {
            Allocator.PrintDebug(this, print);
        }

        public override string ToString() {
            return $"{(IsAllocated ? "Allocated" : "Free")}Region[0x{Offset.ToString("X")} + 0x{Size.ToString("X")}]";
        }
    }

    internal void PrintDebug(Region region, bool print) {
#if DEBUG
        lock (_lock) {
            Region? r = region;
            while (r.Previous != null) r = r.Previous;
            Region? previous = null;
            bool hasErr = false;
            StringBuilder log = new StringBuilder();

            void Msg(string s, bool err) {
                if (err) log.Append("Error: ");
                log.AppendLine(s);
                hasErr |= err;
            }

            Msg($"\nAllocator Status: ", false);
            Msg($"Free list length = {_freeList.Count}\n", false);
            List<Region> regions = new();
            ulong sizeSum = 0;
            while (r != null) {
                regions.Add(r);
                if (r.Previous != previous)
                    Msg(" => Linked list broken here! <=", true);
                Msg($"Region: {r}", false);
                if ((r.Offset & AlignmentMask) != 0) Msg($"Region not aligned.", true);
                if (_freeList.Contains(r) != r.IsFree) Msg($"Wrong free list state for region.", true);
                if (r.Offset != sizeSum) Msg($"Wrong offset in region. Expected {sizeSum}.", true);
                if (_regionPool.Contains(r)) Msg("Element in linked region list is returned.", true);
                Msg($"", false);
                sizeSum += r.Size;
                previous = r;
                r = r.Next;
            }
            if (sizeSum == Size) {
                Msg($"All {sizeSum} elements accounted for", false);
            } else {
                Msg($"{sizeSum} / {Size} elements accounted for", true);
            }

            ulong lastEnd = 0UL;
            for (int i = 0; i < _freeList.Count; i++) {
                Region freeRegion = _freeList[i];
                if (!regions.Contains(freeRegion))
                    Msg($"Orphaned region {freeRegion} in free list.", true);
                ulong end = freeRegion.Offset + freeRegion.Size;
                if (end <= lastEnd)
                    Msg($"Region {freeRegion} in free list not sorted.", true);
                if (freeRegion.FreeIndex != i)
                    Msg($"Region {freeRegion} has wrong free index {freeRegion.FreeIndex}.", true);
                if (_regionPool.Contains(freeRegion)) Msg($"Element {freeRegion} in free list is returned.", true);

            }
            if (hasErr)
                throw new UnreachableException("Allocation structure errors detected.\n" + log);
            if (print)
                Console.WriteLine(log);
        }
#endif
    }
}
