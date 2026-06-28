
using Ember.Collections;

namespace Ember.Tests;

[Collection("UnmanagedTests")]
public class UnmanagedHashSetTests {
    [Fact]
    public void AddValue() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<ulong> set = UnmanagedHashSet<ulong>.Allocate();

            set.Add(1);

            Assert.Equal(set as IEnumerable<ulong>, [1]);
            Assert.Equal(set.Count, 1);

            set.DebugValidate();
        });
    }

    [Fact]
    public void RemoveValue() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<ulong> set = UnmanagedHashSet<ulong>.Allocate();
            Assert.True(set.Add(1));
            Assert.True(set.Add(7729));

            Assert.Equal(set.Count, 2);

            Assert.True(set.Remove(1));
            Assert.Equal(set.Count, 1);

            Assert.False(set.Contains(1));
            Assert.True(set.Contains(7729));

            Assert.Single(set, value => value == 7729);

            Assert.False(set.Remove(1));
            Assert.True(set.Remove(7729));

            Assert.Equal(set.Count, 0);
            Assert.Empty(set);

            Assert.False(set.Remove(7729));

            set.DebugValidate();
        });
    }

    [Fact]
    public void AlreadyExistingValue() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<ulong> set = UnmanagedHashSet<ulong>.Allocate();

            Assert.True(set.Add(1));

            Assert.False(set.Add(1));

            Assert.True(set.Add(2));
            Assert.True(set.Add(3));

            Assert.False(set.Add(2));
            Assert.False(set.Add(3));

            Assert.True(set.Add(4));
            Assert.False(set.Add(1));
            Assert.False(set.Add(2));
            Assert.False(set.Add(3));
            Assert.False(set.Add(4));

            Assert.True(set.Remove(1));
            Assert.False(set.Add(2));
            Assert.False(set.Add(3));
            Assert.False(set.Add(4));
            Assert.True(set.Add(1));

            Assert.True(set.Remove(2));

            Assert.False(set.Contains(2));
            Assert.True(set.Contains(1));
            Assert.True(set.Contains(3));
            Assert.True(set.Contains(4));

            Assert.Equal(set.Order() as IEnumerable<ulong>, [1, 3, 4]);

            set.DebugValidate();
        });
    }

    [Fact]
    public void StressLargeRange() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<ulong> unmanagedSet = UnmanagedHashSet<ulong>.Allocate();
            HashSet<ulong> systemSet = [];

            Random random = new Random(0);

            for (int i = 0; i < 100000; i++) {
                ulong value = (ulong)random.NextInt64();

                if (random.NextDouble() < 0.5) {
                    Assert.Equal(unmanagedSet.Add(value), systemSet.Add(value));
                } else {
                    Assert.Equal(unmanagedSet.Remove(value), systemSet.Remove(value));
                }
            }

            Assert.True(systemSet.All(e => unmanagedSet.Contains(e)));
            Assert.Equal(systemSet.Order(), unmanagedSet.Order());

            unmanagedSet.DebugValidate();
        });
    }

    [Fact]
    public void StressShortRange() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<ulong> unmanagedSet = UnmanagedHashSet<ulong>.Allocate();
            HashSet<ulong> systemSet = [];

            Random random = new Random(0);

            for (int i = 0; i < 100000; i++) {
                ulong value = (ulong)random.Next(10);

                if (random.NextDouble() < 0.5) {
                    Assert.Equal(unmanagedSet.Add(value), systemSet.Add(value));
                } else {
                    Assert.Equal(unmanagedSet.Remove(value), systemSet.Remove(value));
                }
            }

            Assert.True(systemSet.All(e => unmanagedSet.Contains(e)));
            Assert.Equal(systemSet.Order(), unmanagedSet.Order());

            unmanagedSet.DebugValidate();
        });
    }

    [Fact]
    public void StressBadHashLargeRange() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<BadHashValue> unmanagedSet = UnmanagedHashSet<BadHashValue>.Allocate();
            HashSet<BadHashValue> systemSet = [];

            Random random = new Random(0);

            for (int i = 0; i < 10000; i++) {
                ulong value = (ulong)random.NextInt64();

                if (random.NextDouble() < 0.5) {
                    Assert.Equal(unmanagedSet.Add(value), systemSet.Add(value));
                } else {
                    Assert.Equal(unmanagedSet.Remove(value), systemSet.Remove(value));
                }
            }

            Assert.True(systemSet.All(e => unmanagedSet.Contains(e)));
            Assert.Equal(systemSet.Order(), unmanagedSet.Order());

            unmanagedSet.DebugValidate();
        });
    }

    [Fact]
    public void StressBadHashShortRange() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedHashSet<BadHashValue> unmanagedSet = UnmanagedHashSet<BadHashValue>.Allocate();
            HashSet<BadHashValue> systemSet = [];

            Random random = new Random(0);

            for (int i = 0; i < 10000; i++) {
                ulong value = (ulong)random.Next(10);

                Assert.Equal(unmanagedSet.Contains(value), systemSet.Contains(value));

                if (random.NextDouble() < 0.5) {
                    Assert.Equal(unmanagedSet.Add(value), systemSet.Add(value));
                } else {
                    Assert.Equal(unmanagedSet.Remove(value), systemSet.Remove(value));
                }
            }

            Assert.True(systemSet.All(e => unmanagedSet.Contains(e)));
            Assert.Equal(systemSet.Order(), unmanagedSet.Order());

            unmanagedSet.DebugValidate();
        });
    }
}