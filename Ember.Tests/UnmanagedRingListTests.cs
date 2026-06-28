namespace Ember.Tests;

using System.Collections.Generic;
using Ember.Collections;

[Collection("UnmanagedTests")]
public class UnmanagedRingListTests {
    [Fact]
    public void PushPop_Front_Single() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            Assert.Equal(0, list.Count);
            list.PushFront(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list[0]);
            Assert.Equal(7729UL, list.PopFront());
            Assert.Equal(0, list.Count);
        });
    }

    [Fact]
    public void PushPop_Back_Single() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            Assert.Equal(0, list.Count);
            list.PushBack(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list[0]);
            Assert.Equal(7729UL, list.PopBack());
            Assert.Equal(0, list.Count);
        });
    }

    [Fact]
    public void PushPop_Mixed_Single() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            Assert.Equal(0, list.Count);
            list.PushBack(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list[0]);
            Assert.Equal(7729UL, list.PopFront());
            Assert.Equal(0, list.Count);

            list.PushFront(420);
            Assert.Equal(1, list.Count);
            Assert.Equal(420UL, list[0]);
            Assert.Equal(420UL, list.PopBack());
            Assert.Equal(0, list.Count);
        });
    }

    [Fact]
    public void PushPop_Front_Loop() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            for (int i = 0; i < 6; i++) {
                Assert.Equal(i, list.Count);
                list.PushFront(7729);
                Assert.Equal(i + 1, list.Count);
                Assert.Equal(7729UL, list.PopFront());
                Assert.Equal(i, list.Count);

                list.PushFront(69);
            }

            Assert.Equal(new ulong[] { 69, 69, 69, 69, 69, 69 }, list);

            for (int i = 0; i < 6; i++) {
                Assert.Equal(6 - i, list.Count);
                Assert.Equal(69UL, list.PopFront());
            }

            Assert.Equal(0, list.Count);

            list.PushFront(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list.PopFront());
        });
    }

    [Fact]
    public void PushPop_Back_Loop() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            for (int i = 0; i < 6; i++) {
                Assert.Equal(i, list.Count);
                list.PushBack(7729);
                Assert.Equal(i + 1, list.Count);
                Assert.Equal(7729UL, list.PopBack());
                Assert.Equal(i, list.Count);

                list.PushBack(69);
            }

            Assert.Equal(new ulong[] { 69, 69, 69, 69, 69, 69 }, list);

            for (int i = 0; i < 6; i++) {
                Assert.Equal(6 - i, list.Count);
                Assert.Equal(69UL, list.PopBack());
            }

            Assert.Equal(0, list.Count);

            list.PushBack(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list.PopBack());
        });
    }

    [Fact]
    public void PushPop_Mixed_Loop() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedRingList<ulong> list = UnmanagedRingList<ulong>.Allocate(4);

            for (int i = 0; i < 3; i++) {
                Assert.Equal(i * 2, list.Count);
                list.PushFront(7729);
                Assert.Equal(i * 2 + 1, list.Count);
                Assert.Equal(7729UL, list.PopFront());
                Assert.Equal(i * 2, list.Count);
                list.PushBack(7729);
                Assert.Equal(i * 2 + 1, list.Count);
                Assert.Equal(7729UL, list.PopBack());

                list.PushBack(69);
                list.PushFront(420);
            }

            Assert.Equal(new ulong[] { 69, 69, 69, 420, 420, 420 }, list);

            for (int i = 0; i < 3; i++) {
                Assert.Equal(6 - i * 2, list.Count);
                Assert.Equal(69UL, list.PopBack());
                Assert.Equal(420UL, list.PopFront());
            }

            Assert.Equal(0, list.Count);

            list.PushBack(7729);
            Assert.Equal(1, list.Count);
            Assert.Equal(7729UL, list.PopBack());
        });
    }
}