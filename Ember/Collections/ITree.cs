
using System.Collections.Generic;

namespace Ember.Collections;

public interface ITree<T, Branch, Leaf, Node>
    where Branch : ITreeBranch<T, Branch, Leaf, Node>
    where Leaf : ITreeLeaf<T, Branch, Leaf, Node>
    where Node : ITreeNode<T, Branch, Leaf, Node> {

    public Node RootNode { get; }

    public IEnumerable<T> AllValues { get; }
}

public interface ITreeNode<T, Branch, Leaf, Node>
    where Branch : ITreeBranch<T, Branch, Leaf, Node>
    where Leaf : ITreeLeaf<T, Branch, Leaf, Node>
    where Node : ITreeNode<T, Branch, Leaf, Node> {

    public bool IsLeaf { get; }
    public bool IsBranch => !IsLeaf;

    public IEnumerable<T> AllValues { get; }
    public IEnumerable<Node> AllChildren { get; }

    public Branch AsBranch();
    public Leaf AsLeaf();

    public Branch SetBranch(T leafValues);
    public Leaf SetLeaf(T value);
}

public interface ITreeBranch<T, Branch, Leaf, Node>
    where Branch : ITreeBranch<T, Branch, Leaf, Node>
    where Leaf : ITreeLeaf<T, Branch, Leaf, Node>
    where Node : ITreeNode<T, Branch, Leaf, Node> {
    public Node AsNode();

    public IReadOnlyCollection<Node> Children { get; }
}

public interface ITreeLeaf<T, Branch, Leaf, Node>
    where Branch : ITreeBranch<T, Branch, Leaf, Node>
    where Leaf : ITreeLeaf<T, Branch, Leaf, Node>
    where Node : ITreeNode<T, Branch, Leaf, Node> {
    public Node AsNode();

    public T Value { get; set; }
}