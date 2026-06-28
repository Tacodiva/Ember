
namespace Ember.Tests;

using System.Diagnostics;
using Ember.Collections;
using Ember.Math;

[Collection("UnmanagedTests")]
public class UnmanagedOctreeTests {

    [Fact]
    public void SetGet_Simple() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedOctree<long> tree = UnmanagedOctree<long>.Allocate(3, 0);

            Assert.Equal(0, tree.Get((5, 6, 6)));
            tree.Set((5, 6, 6), 69);
            Assert.Equal(69, tree.Get((5, 6, 6)));
            Assert.Equal(0, tree.Get((4, 6, 6)));
            tree.Set((5, 6, 6), 0);
            Assert.Equal(0, tree.Get((5, 6, 6)));
            tree.Set((0, 0, 0), 69);
            tree.Set((7, 7, 7), 420);
            tree.Set((4, 4, 4), 7729);
            Assert.Equal(69, tree.Get((0, 0, 0)));
            Assert.Equal(420, tree.Get((7, 7, 7)));
            Assert.Equal(7729, tree.Get((4, 4, 4)));
            Assert.Equal(0, tree.Get((5, 6, 6)));
        });
    }

    [Fact]
    public void SetGet_Stress() {
        TestUtils.CheckMemoryFreed(() => {
            int sizeLog2 = 3;
            int size = 1 << sizeLog2;
            long defaultValue = 10;

            using UnmanagedOctree<long> tree = UnmanagedOctree<long>.Allocate(sizeLog2, defaultValue);

            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {
                    for (int z = 0; z < size; z++) {
                        Assert.Equal(defaultValue, tree.Get((x, y, z)));
                    }
                }
            }

            Random random = new(0);
            Dictionary<Vec3i, long> setPoints = new();

            for (int iter = 0; iter < 1000; iter++) {
                for (int i = 0; i < iter; i++) {
                    Vec3i point = (random.Next(size), random.Next(size), random.Next(size));
                    long value = random.NextInt64();
                    setPoints[point] = value;
                    tree.Set(point, value);
                }

                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        for (int z = 0; z < size; z++) {
                            if (setPoints.TryGetValue((x, y, z), out long value))
                                Assert.Equal(value, tree.Get((x, y, z)));
                            else
                                Assert.Equal(defaultValue, tree.Get((x, y, z)));
                        }
                    }
                }

                {
                    List<long> values = setPoints.Values.ToList();
                    values.RemoveAll(v => v == defaultValue);
                    foreach (long child in tree.RootNode.AllValues) {
                        if (child != defaultValue)
                            Debug.Assert(values.Remove(child));
                    }
                    Assert.Empty(values);
                }
            }
        });
    }

    [Fact]
    public void NodeEnumeration() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedOctree<long> tree = UnmanagedOctree<long>.Allocate(3, 10);

            Assert.Equal(tree.AllValues, new long[] { 10 });
            Assert.True(tree.RootNode.IsLeaf);
            Assert.False(tree.RootNode.IsBranch);

            tree.RootNode.SetBranch(69);

            Assert.True(tree.RootNode.IsBranch);
            Assert.False(tree.RootNode.IsLeaf);
            Assert.Equal(8, tree.RootNode.AllChildren.Count());
            List<long> expectedChildren = new() { 69, 69, 69, 69, 69, 69, 69, 69 };

            Assert.Equal(expectedChildren, tree.AllValues);
            Assert.Equal(expectedChildren, tree.RootNode.AsBranch().Children.Select(child => child.AsLeaf().Value));

            UnmanagedOctree<long>.Branch branch = tree.RootNode.AsBranch();

            branch.Children[0].SetLeaf(420);
            expectedChildren[0] = 420;

            branch.Children[7].AsLeaf().Value = 90;
            expectedChildren[7] = 90;

            Assert.True(tree.RootNode.IsBranch);
            Assert.False(tree.RootNode.IsLeaf);

            Assert.Equal(8, tree.RootNode.AllChildren.Count());
            Assert.Equal(expectedChildren, tree.AllValues);
            Assert.Equal(expectedChildren, tree.RootNode.AsBranch().Children.Select(child => child.AsLeaf().Value));

            UnmanagedOctree<long>.Branch child1Branch = branch.Children[1].SetBranch(1111);

            child1Branch.Children[2].SetLeaf(2222);
            List<long> childBranchChildren = new() { 1111, 1111, 2222, 1111, 1111, 1111, 1111, 1111 };
            expectedChildren.RemoveAt(1);
            expectedChildren.InsertRange(1, childBranchChildren);

            Assert.Equal(16, tree.RootNode.AllChildren.Count());
            Assert.Equal(expectedChildren, tree.AllValues);
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AsBranch().Children.Select(child => child.AsLeaf().Value));
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AllChildren.Select(child => child.AsLeaf().Value));
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AllValues);

            child1Branch = branch.Children[1].SetBranch(3333);

            child1Branch.Children[6].SetLeaf(4444);
            childBranchChildren = new() { 3333, 3333, 3333, 3333, 3333, 3333, 4444, 3333 };
            for (int i = 0; i < 8; i++) expectedChildren.RemoveAt(1);
            expectedChildren.InsertRange(1, childBranchChildren);

            Assert.Equal(16, tree.RootNode.AllChildren.Count());
            Assert.Equal(expectedChildren, tree.AllValues);
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AsBranch().Children.Select(child => child.AsLeaf().Value));
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AllChildren.Select(child => child.AsLeaf().Value));
            Assert.Equal(childBranchChildren, tree.RootNode.AsBranch().Children[1].AllValues);

            UnmanagedOctree<long>.Leaf child1Leaf = child1Branch.AsNode().SetLeaf(-3333);
            for (int i = 0; i < 8; i++) expectedChildren.RemoveAt(1);
            expectedChildren.Insert(1, -3333);

            Assert.Equal(8, tree.RootNode.AllChildren.Count());
            Assert.Equal(expectedChildren, tree.AllValues);
            Assert.Equal(expectedChildren, tree.RootNode.AsBranch().Children.Select(child => child.AsLeaf().Value));

            tree.RootNode.SetLeaf(7729);
            Assert.True(tree.RootNode.IsLeaf);
            Assert.False(tree.RootNode.IsBranch);
            Assert.Equal(new long[] { 7729 }, tree.AllValues);
            Assert.Equal(7729, tree.RootNode.AsLeaf().Value);
            Assert.Empty(tree.RootNode.AllChildren);
        });
    }

}