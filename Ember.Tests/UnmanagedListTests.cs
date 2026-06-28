namespace Ember.Tests;

using System.Collections.Generic;
using Ember.Collections;

[Collection("UnmanagedTests")]
public class UnmanagedListTests {
    [Fact]
    public void Add_AddsValueToList() {
        TestUtils.CheckMemoryFreed(() => {

            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();

            list.Add(1);

            Assert.Equal(1UL, list[0]);
        });
    }

    [Fact]
    public void Remove_RemovesValueFromList() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();
            list.Add(1);
            list.Add(2);

            list.Remove(1);

            Assert.Single(list);
            Assert.DoesNotContain(1UL, list);
        });
    }

    [Fact]
    public void Count_ReturnsCorrectNumberOfItemsInList() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();
            list.Add(1UL);
            list.Add(2UL);
            list.Add(3UL);

            list.Remove(2UL);

            int count = list.Count;

            Assert.Equal(2, count);
        });
    }

    [Fact]
    public void Clear_RemovesAllItemsFromList() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.Clear();

            Assert.Empty(list);
        });
    }


    [Fact]
    public void GetReference_Works() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();
            ulong item = 420;
            list.Add(item);
            list.GetRef(0) = 69UL;
            Assert.Equal(69UL, list.Get(0));
        });
    }

    [Fact]
    public void TryGetValue_StressTest() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<int> list = UnmanagedList<int>.Allocate();
            List<int> realList = new();
            Random random = new Random(0);

            for (int i = 0; i < 100000; i++) {
                int value = random.Next(1000);
                if (random.NextDouble() < 0.25 && list.Count != 0) {
                    int index = random.Next(list.Count);
                    list.Insert(index, value);
                    realList.Insert(index, value);
                } else {
                    if (random.NextDouble() < 0.5) {
                        list.Add(value);
                        realList.Add(value);
                    } else {
                        Assert.Equal(list.Remove(value), realList.Remove(value));
                    }
                }
                Assert.Equal(realList.Count, list.Count);
            }

            Assert.Equal(realList, list);
        });
    }

    [Fact]
    public void Enumerator_ReturnsAllValuesInList() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();
            list.Add(1UL);
            list.Add(2UL);
            list.Add(3UL);

            List<ulong> values = new List<ulong>();
            foreach (ulong value in list) {
                values.Add(value);
            }

            Assert.Equal(3, values.Count);
            Assert.Contains(1UL, values);
            Assert.Contains(2UL, values);
            Assert.Contains(3UL, values);
        });
    }

    [Fact]
    public void Enumerator_ReturnsNoValuesForEmptyList() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedList<ulong> list = UnmanagedList<ulong>.Allocate();

            list.Add(1UL);
            list.Clear();

            List<ulong> values = new List<ulong>();
            foreach (ulong value in list) {
                values.Add(value);
            }

            Assert.Empty(values);
        });
    }
}