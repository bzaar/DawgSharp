using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawgSharp;

class YaleGraph
{
    private readonly int rootNodeIndex;
    private readonly char firstChar;
    private readonly char lastChar;
    private readonly ushort[] charToIndexPlusOne;
    private readonly int[] firstChildForNode;
    private readonly YaleChild[] children;
    private readonly char[] indexToChar;

    public YaleGraph(YaleChild[] children,
        int[] firstChildForNode,
        ushort[] charToIndexPlusOne,
        int rootNodeIndex,
        char[] indexToChar)
    {
        this.children = children;
        this.firstChildForNode = firstChildForNode;
        this.charToIndexPlusOne = charToIndexPlusOne;
        this.indexToChar = indexToChar;
        this.lastChar = this.indexToChar.LastOrDefault();
        this.firstChar = this.indexToChar.FirstOrDefault();
        this.rootNodeIndex = rootNodeIndex;
    }

    public int NodeCount => firstChildForNode.Length - 1;
        
    public IEnumerable<int> GetPath(IEnumerable<char> word)
    {
        int nodeIndex = rootNodeIndex;

        yield return nodeIndex;

        if (nodeIndex == -1)
        {
            yield break;
        }

        foreach (char c in word)
        {
            int childIndex = GetChildIndex(nodeIndex, c);

            if (childIndex >= 0)
            {
                nodeIndex = children[childIndex].Index;

                yield return nodeIndex;
                continue;
            }
                
            yield return -1;
            yield break;
        }
    }

    private int GetChildIndex(int nodeIndex, char c)
    {
        if (c >= firstChar && c <= lastChar)
        {
            ushort charIndexPlusOne = charToIndexPlusOne[c - firstChar];

            if (charIndexPlusOne != 0)
            {
                int firstChildIndex = firstChildForNode[nodeIndex];

                int lastChildIndex = firstChildForNode[nodeIndex + 1];

                int nChildren = lastChildIndex - firstChildIndex;

                var charIndex = (ushort)(charIndexPlusOne - 1);

                int childIndex;
            
                if (nChildren == 1)
                {
                    childIndex = children[firstChildIndex].CharIndex == charIndex ? firstChildIndex : -1;
                }
                else
                {
                    var searchValue = new YaleChild(-1, charIndex);

                    childIndex = Array.BinarySearch(children, firstChildIndex, nChildren, searchValue, childComparer);
                }

                return childIndex;
            }
        }
            
        return -1;
    }

    public IEnumerable<KeyValuePair<string, int>> MatchTree(IEnumerable<IEnumerable<char>> tree)
    {
        int nodeIndex = rootNodeIndex;

        var stack = new Stack<Frame>();

        var enums = tree.ToList();

        var sb = new StringBuilder(enums.Count);

        IEnumerator<char> enumerator = null;
            
        for (;;)
        {
            if (enumerator != null)
            {
                int childIndex = -1;
                while (enumerator.MoveNext())
                {
                    childIndex = GetChildIndex(nodeIndex, enumerator.Current);

                    if (childIndex >= 0)
                    {
                        break;
                    }
                }
                    
                if (childIndex >= 0)
                {
                    sb.Append(enumerator.Current);
                    stack.Push(new Frame(nodeIndex, enumerator));
                    nodeIndex = children[childIndex].Index;
                    enumerator = null;
                }
                else
                {
                    enumerator.Dispose();
                        
                    if (stack.Count == 0) yield break;
                        
                    (nodeIndex, enumerator) = stack.Pop();
                    --sb.Length;
                }
            }
            else if (stack.Count < enums.Count)
            {
                enumerator = enums[stack.Count].GetEnumerator();
            }
            else
            {
                yield return new KeyValuePair<string, int>(sb.ToString(), nodeIndex);
                    
                if (stack.Count == 0) yield break;
                        
                (nodeIndex, enumerator) = stack.Pop();
                --sb.Length;
            }
        }
    }

    class Frame
    {
        public Frame(int nodeIndex, IEnumerator<char> enumerator)
        {
            this.NodeIndex = nodeIndex;
            this.Enumerator = enumerator;
        }

        public int NodeIndex { get; }
        public IEnumerator<char> Enumerator { get; }

        public void Deconstruct(out int nodeIndex, out IEnumerator<char> enumerator)
        {
            nodeIndex = this.NodeIndex;
            enumerator = this.Enumerator;
        }
    }

    private static readonly ChildComparer childComparer = new();
        
    class ChildComparer : IComparer<YaleChild>
    {
        public int Compare(YaleChild x, YaleChild y)
        {
            return x.CharIndex.CompareTo(y.CharIndex);
        }
    }

    public bool IsLeaf(int nodeIndex)
    {
        return firstChildForNode[nodeIndex] == firstChildForNode[nodeIndex + 1];
    }
        
    public IEnumerable<int> MatchPrefix (StringBuilder sb, int nodeIndex)
    {
        if (nodeIndex != -1)
        {
            yield return nodeIndex;
                
            int firstChildIndex = firstChildForNode [nodeIndex];

            int lastChildIndex = firstChildForNode[nodeIndex + 1];

            for (int i = firstChildIndex; i < lastChildIndex; ++i)
            {
                YaleChild child = children [i];

                sb.Append (indexToChar [child.CharIndex]);

                foreach (var child_nodeIndex in MatchPrefix (sb, child.Index))
                {
                    yield return child_nodeIndex;
                }

                --sb.Length;
            }
        }
    }
}