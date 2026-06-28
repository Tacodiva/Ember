
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Ember.Math;
using Ember.Memory;
using static Ember.Collections.UnmanagedOctree.InternalNode;
using static Ember.Collections.UnmanagedOctree;

namespace Ember.Collections;

// Cannot be inside the main class because generic types can't have explicit layouts
internal static class UnmanagedOctree {
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    internal struct InternalNode {
        internal enum NodeType : int {
            Branch,
            Leaf
        }

        [FieldOffset(0)] public NodeType Type;

        [FieldOffset(4)] public int BranchIndex;
        [FieldOffset(4)] public long LeafValueLong;
    }
}

public unsafe readonly struct UnmanagedOctree<T> :
    ITree<T, UnmanagedOctree<T>.Branch, UnmanagedOctree<T>.Leaf, UnmanagedOctree<T>.Node>,
    IDisposable where T : unmanaged {

    public static UnmanagedOctree<T> Null => default;

    public static UnmanagedOctree<T> Allocate(int depth, T defaultValue = default) {
        return new(depth, defaultValue);
    }

    internal ref struct InternalNodePtr {
        public InternalNode* Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InternalNodePtr(InternalNode* ptr) {
            Ptr = ptr;
        }

#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SetLeaf(UnmanagedOctree<T> tree, T value) {
            Type = NodeType.Leaf;
            LeafValue = value;
        }

#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public InternalNodePtr SetBranch(UnmanagedOctree<T> tree, int parentIndex, int parentChildIndex, T fillValue) {
            int branchIndex;

            UnmanagedList<int> freeBranches = tree._ptr->FreeBranches;
            if (!freeBranches.IsNull && freeBranches.Count != 0) {
                branchIndex = freeBranches.Pop();
            } else {
                if (!tree._ptr->Branches.IsNull) {
                    branchIndex = BranchIndex = tree._ptr->Branches.Count;
                } else {
                    tree._ptr->Branches = UnmanagedList<InternalBranchData>.Allocate();
                    branchIndex = 0;
                }

                tree._ptr->Branches.Add(default);

                if (parentIndex != -1) {
                    InternalBranchDataPtr parentData = tree.GetBranchData(parentIndex);
                    Ptr = parentData.GetChild(parentChildIndex).Ptr;
                }
            }



            Type = NodeType.Branch;
            BranchIndex = branchIndex;

            InternalBranchDataPtr data = tree.GetBranchData(branchIndex);
            data.ParentBranchIndex = parentIndex;
            data.ParentChildIndex = parentChildIndex;
            ++data.ModificationNumber;

            for (int i = 0; i < 8; i++) data.GetChild(i).SetLeaf(tree, fillValue);

            return this;
        }

        public NodeType Type {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return Ptr->Type;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Ptr->Type = value;
            }
        }

        public int BranchIndex {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return Ptr->BranchIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Ptr->BranchIndex = value;
            }
        }

        public T* LeafValuePtr {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return (T*)(&Ptr->LeafValueLong);
            }
        }

        public T LeafValue {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return *(T*)(&Ptr->LeafValueLong);
            }

            set {
                *(T*)(&Ptr->LeafValueLong) = value;
            }
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct InternalBranchData {
        public InternalNode Child0;
        public InternalNode Child1;
        public InternalNode Child2;
        public InternalNode Child3;
        public InternalNode Child4;
        public InternalNode Child5;
        public InternalNode Child6;
        public InternalNode Child7;

        public int ParentBranchIndex;
        public int ParentChildIndex;
        /// <summary>
        /// Increments when the parent or child index is change or this data is marked as free.
        /// </summary>
        public long ModificationNumber;
    }

    internal ref struct InternalBranchDataPtr {
        public InternalBranchData* Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InternalBranchDataPtr(InternalBranchData* ptr) {
            Ptr = ptr;
        }

        public int ParentBranchIndex {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return Ptr->ParentBranchIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Ptr->ParentBranchIndex = value;
            }
        }

        public int ParentChildIndex {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return Ptr->ParentChildIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Ptr->ParentChildIndex = value;
            }
        }

        public long ModificationNumber {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return Ptr->ModificationNumber;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Ptr->ModificationNumber = value;
            }
        }

#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public InternalNodePtr GetChild(int index) {
            if (index > 7 || index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return new(&(&Ptr->Child0)[index]);
        }
    }

    private struct InternalData {
        public InternalNode Root;
        public UnmanagedList<InternalBranchData> Branches;
        public UnmanagedList<int> FreeBranches;
        public int Depth;
    }

    private readonly InternalData* _ptr;
    public bool IsNull => _ptr == null;

    public int Depth => _ptr->Depth;
    public int Size => 1 << _ptr->Depth;
    public int Volume => Size * Size * Size;

    public Node RootNode => new(new NodeReference(this, -1, -1));
    private InternalNodePtr RootPtr => new(&_ptr->Root);

    public Node.ValueEnumerable AllValues => new(RootNode);
    IEnumerable<T> ITree<T, Branch, Leaf, Node>.AllValues => AllValues;

    private UnmanagedOctree(int depth, T defaultValue) {
        if (sizeof(T) > sizeof(long)) throw new ArgumentException($"Cannot make an octree of type '{typeof(T)}'. Size must be not be larger than {sizeof(long)} bytes (got {sizeof(T)} bytes)");
        _ptr = MemoryUtils.AllocateUninitialized<InternalData>();
        _ptr->Depth = depth;
        _ptr->Branches = default;
        _ptr->FreeBranches = default;

        RootPtr.SetLeaf(this, defaultValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T* TraverseTree(Vec3i position, bool mutate) {
        return TraverseTree(new(&_ptr->Root), -1, -1, position, _ptr->Depth, mutate);
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private T* TraverseTree(InternalNodePtr node, int parentIndex, int parentChildIndex, Vec3i pos, int depth, bool mutate) {
        switch (node.Type) {
            case NodeType.Branch:
                return TraverseTreeBranch(node, pos, depth, mutate);
            case NodeType.Leaf: {
                    if (mutate && depth != 0) {
                        node.SetBranch(this, parentIndex, parentChildIndex, node.LeafValue);
                        return TraverseTreeBranch(node, pos, depth, mutate);
                    } else {
                        return (T*)&node.Ptr->LeafValueLong;
                    }
                }
            default:
                throw new InvalidOperationException();
        }
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private T* TraverseTreeBranch(InternalNodePtr nodePtr, Vec3i pos, int depth, bool mutate) {
        int newDepth = depth - 1;
        Vec3i indexBits = (pos & (1 << newDepth)) >> newDepth;
        int parentChildIndex = indexBits.Z << 2 | indexBits.Y << 1 | indexBits.X;

        int parentBranchIndex = nodePtr.BranchIndex;
        InternalBranchDataPtr branchData = GetBranchData(parentBranchIndex);

        return TraverseTree(branchData.GetChild(parentChildIndex), parentBranchIndex, parentChildIndex, pos, newDepth, mutate);
    }

    public ref T GetRef(Vec3i position) {
        return ref Unsafe.AsRef<T>(TraverseTree(position, true));
    }

    public T Get(Vec3i position) {
        return *TraverseTree(position, false);
    }

    public void Set(Vec3i position, T value) {
        *TraverseTree(position, true) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InternalBranchDataPtr GetBranchData(int index) {
        return new(_ptr->Branches.GetPointer(index));
    }

    public void Dispose() {
        if (!IsNull) {
            if (!_ptr->Branches.IsNull) _ptr->Branches.Dispose();
            if (!_ptr->FreeBranches.IsNull) _ptr->FreeBranches.Dispose();
            MemoryUtils.Free(_ptr);
        }
    }

    private void DestroyNode(InternalNodePtr pNode) {
        if (pNode.Type == NodeType.Branch) {

            InternalBranchDataPtr branchData = GetBranchData(pNode.BranchIndex);
            for (int i = 0; i < 8; i++) DestroyNode(branchData.GetChild(i));

            ++branchData.ModificationNumber;

            UnmanagedList<int> freeBranches = _ptr->FreeBranches;
            if (freeBranches.IsNull)
                freeBranches = _ptr->FreeBranches = UnmanagedList<int>.Allocate();
            freeBranches.Add(pNode.BranchIndex);
        }
    }

    internal readonly struct NodeReference {
        public readonly UnmanagedOctree<T> Tree;
        public readonly int ParentBranchIndex;
        public readonly int ParentChildIndex;

        public InternalNodePtr Node {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (ParentBranchIndex == -1) return new(&Tree._ptr->Root);
                return Tree.GetBranchData(ParentBranchIndex).GetChild(ParentChildIndex);
            }
        }

        public InternalBranchDataPtr ParentBranchData {
            get {
                if (ParentBranchIndex == -1) throw new NullReferenceException();
                return Tree.GetBranchData(ParentBranchIndex);
            }
        }

        public NodeReference(UnmanagedOctree<T> tree, int parentBranchIndex, int parentChildIndex) {
            Tree = tree;
            ParentBranchIndex = parentBranchIndex;
            ParentChildIndex = parentChildIndex;
        }
    };

    public readonly struct Node : ITreeNode<T, Branch, Leaf, Node> {
        public bool IsLeaf {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return _nodeRef.Node.Type == NodeType.Leaf;
            }
        }

        public bool IsBranch {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return _nodeRef.Node.Type == NodeType.Branch;
            }
        }

        private readonly NodeReference _nodeRef;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Node(NodeReference nodeData) {
            _nodeRef = nodeData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Branch AsBranch() {
            if (!IsBranch) throw new InvalidOperationException("Node is not a branch.");
            return new(_nodeRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Leaf AsLeaf() {
            if (!IsLeaf) throw new InvalidOperationException("Node is not a leaf.");
            return new(_nodeRef);
        }

        public Branch SetBranch(T fillValue = default) {
            if (IsBranch) {
                int branchIndex = _nodeRef.Node.BranchIndex;
                InternalBranchDataPtr branchData = _nodeRef.Tree.GetBranchData(branchIndex);
                for (int i = 0; i < 8; i++) _nodeRef.Tree.DestroyNode(branchData.GetChild(i));
                ++branchData.ModificationNumber;

                for (int i = 0; i < 8; i++) branchData.GetChild(i).SetLeaf(_nodeRef.Tree, fillValue);
            } else {
                _nodeRef.Tree.DestroyNode(_nodeRef.Node);
                _nodeRef.Node.SetBranch(_nodeRef.Tree, _nodeRef.ParentBranchIndex, _nodeRef.ParentChildIndex, fillValue);
            }

            return AsBranch();
        }

        public Leaf SetLeaf(T value = default) {
            if (IsLeaf) {
                *_nodeRef.Node.LeafValuePtr = value;
            } else {
                _nodeRef.Tree.DestroyNode(_nodeRef.Node);
                _nodeRef.Node.SetLeaf(_nodeRef.Tree, value);
            }

            return AsLeaf();
        }

        public ChildEnumerable AllChildren => new(this);
        IEnumerable<Node> ITreeNode<T, Branch, Leaf, Node>.AllChildren => AllChildren;

        public readonly struct ChildEnumerable : IEnumerable<Node> {
            public readonly Node Node;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ChildEnumerable(Node node) {
                Node = node;
            }

            public Enumerator GetEnumerator() => new(Node);
            IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<Node> {
                public readonly Node Current {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get {
                        if (_branchIndex == -1) throw new InvalidOperationException();
                        return new(new NodeReference(_root._nodeRef.Tree, _branchIndex, _branchChildIndex));
                    }
                }

                readonly object IEnumerator.Current => Current;

                private int _branchChildIndex;
                /// <summary>
                /// The index of the current branch, or -1 if the current branch is null.
                /// </summary>
                private int _branchIndex;
                private long _branchModificationNumber;
                private readonly Node _root;
                private readonly long _rootModificationNumber;
                public readonly bool IsNull => _root._nodeRef.Tree.IsNull;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(Node root) {
                    _root = root;
                    if (root.IsBranch) _rootModificationNumber = _root._nodeRef.Tree.GetBranchData(_root._nodeRef.Node.BranchIndex).ModificationNumber;
                    else _branchIndex = -1;
                    Reset();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private InternalBranchDataPtr ChangeBranch(int branchIndex, int branchChildIndex) {
                    _branchIndex = branchIndex;
                    _branchChildIndex = branchChildIndex;
                    InternalBranchDataPtr data = _root._nodeRef.Tree.GetBranchData(_branchIndex);
                    _branchModificationNumber = data.ModificationNumber;
                    return data;
                }

                public bool MoveNext() {
                    if (_branchIndex == -1) return false;

                    InternalBranchDataPtr currentBranch = _root._nodeRef.Tree.GetBranchData(_branchIndex);
                    if (currentBranch.ModificationNumber != _branchModificationNumber)
                        throw new InvalidOperationException("Octree node was modified; enumeration operation may not execute");

                    // Push
                    if (_branchChildIndex != -1) {
                        InternalNodePtr currentNode = currentBranch.GetChild(_branchChildIndex);
                        if (currentNode.Type == NodeType.Branch) {
                            ChangeBranch(currentNode.BranchIndex, 0);
                            return true;
                        }
                    }

                    ++_branchChildIndex;

                    // Pop
                    while (_branchChildIndex == 8) {
                        if (_branchIndex == _root._nodeRef.Node.BranchIndex) {
                            --_branchChildIndex;
                            return false;
                        }

                        currentBranch = ChangeBranch(currentBranch.ParentBranchIndex, currentBranch.ParentChildIndex + 1);
                    }

                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() {
                    if (_branchIndex != -1) {
                        ChangeBranch(_root._nodeRef.Node.BranchIndex, -1);
                        if (_branchModificationNumber != _rootModificationNumber)
                            throw new InvalidOperationException("Octree root node was modified; enumeration operation may not execute");

                    }
                }

                public readonly void Dispose() { }

            }
        }

        public ValueEnumerable AllValues => new(this);
        IEnumerable<T> ITreeNode<T, Branch, Leaf, Node>.AllValues => AllValues;

        public readonly struct ValueEnumerable : IEnumerable<T> {
            public readonly Node Node;

            internal ValueEnumerable(Node node) {
                Node = node;
            }

            public Enumerator GetEnumerator() => new(Node);
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<T> {

                public readonly T Current {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get {
                        if (_childEnumerator.IsNull) return _node._nodeRef.Node.LeafValue;
                        return _childEnumerator.Current._nodeRef.Node.LeafValue;
                    }
                }

                readonly object IEnumerator.Current => Current;

                private ChildEnumerable.Enumerator _childEnumerator;
                private bool _hasReturnedNode;
                private readonly Node _node;

                internal Enumerator(Node node) {
                    _node = node;
                    if (_node.IsBranch)
                        _childEnumerator = new ChildEnumerable(node).GetEnumerator();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() {
                    if (_childEnumerator.IsNull) {
                        if (_hasReturnedNode) return false;
                        return _hasReturnedNode = true;
                    }

                    bool hasNext;
                    while ((hasNext = _childEnumerator.MoveNext()) && _childEnumerator.Current.IsBranch) ;

                    return hasNext;
                }

                public void Reset() {
                    _hasReturnedNode = false;
                    _childEnumerator.Reset();
                }

                public readonly void Dispose() { }
            }
        }
    }

    public readonly struct Branch : ITreeBranch<T, Branch, Leaf, Node> {
        private readonly ChildList _children;
        public ChildList Children => _children;
        IReadOnlyCollection<Node> ITreeBranch<T, Branch, Leaf, Node>.Children => Children;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Branch(NodeReference nodeRef) {
            _children = new ChildList(nodeRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node AsNode() => new(_children.NodeRef);

        public readonly struct ChildList : IReadOnlyList<Node> {
            internal readonly NodeReference NodeRef;
            public int Count => 8;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ChildList(NodeReference nodeRef) {
                NodeRef = nodeRef;
            }

            public Node this[int index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get {
                    return Get(index);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node Get(int index) {
                return new(new(NodeRef.Tree, NodeRef.Node.BranchIndex, index));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                return new Enumerator(NodeRef.Tree, NodeRef.Node.BranchIndex);
            }

            IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<Node> {
                private int _index;
                private readonly int _branchIndex;
                private readonly UnmanagedOctree<T> _tree;

                public readonly Node Current {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get {
                        if (_index == -1) throw new InvalidOperationException();
                        return new(new(_tree, _branchIndex, _index));
                    }
                }

                readonly object IEnumerator.Current => Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnmanagedOctree<T> tree, int branchIndex) {
                    _branchIndex = branchIndex;
                    _tree = tree;
                    _index = -1;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() {
                    if (_index == 7) return false;
                    ++_index;
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() {
                    _index = -1;
                }

                public readonly void Dispose() { }
            }
        }
    }

    public readonly struct Leaf : ITreeLeaf<T, Branch, Leaf, Node> {
        private readonly NodeReference _nodeRef;

        public T Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get {
                return _nodeRef.Node.LeafValue;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                *_nodeRef.Node.LeafValuePtr = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Leaf(NodeReference nodeData) {
            _nodeRef = nodeData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node AsNode() => new(_nodeRef);
    }

#if DEBUG
    public void DebugPrint() {
        static StringBuilder CreateText(UnmanagedOctree<T> octree, InternalNodePtr node) {
            StringBuilder sb = new();

            if (node.Type == NodeType.Leaf) {
                sb.Append("\nLeaf " + node.Ptr->LeafValueLong);
            } else {
                InternalBranchDataPtr branchData = octree.GetBranchData(node.BranchIndex);

                sb.Append($"\nBranch {node.BranchIndex} (parent = {branchData.ParentBranchIndex}[{branchData.ParentChildIndex}])");

                for (int i = 0; i < 8; i++) {
                    sb.Append(CreateText(octree, branchData.GetChild(i)).Replace("\n", "\n  |-"));
                }
                sb.Append("\n  *");
            }

            return sb;
        }
        Console.WriteLine(CreateText(this, new(&_ptr->Root)));
    }
#endif
}

