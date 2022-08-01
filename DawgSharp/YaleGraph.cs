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
        int node_i = rootNodeIndex;

        yield return node_i;

        if (node_i == -1)
        {
            yield break;
        }

        foreach (char c in word)
        {
            int child_i = GetChildIndex(node_i, c);

            if (child_i >= 0)
            {
                node_i = children[child_i].Index;

                yield return node_i;
                continue;
            }
                
            yield return -1;
            yield break;
        }
    }

    private int GetChildIndex(int node_i, char c)
    {
        if (c >= firstChar && c <= lastChar)
        {
            ushort charIndexPlusOne = charToIndexPlusOne[c - firstChar];

            if (charIndexPlusOne != 0)
            {
                int firstChild_i = firstChildForNode[node_i];

                int lastChild_i = firstChildForNode[node_i + 1];

                int nChildren = lastChild_i - firstChild_i;

                var charIndex = (ushort)(charIndexPlusOne - 1);

                int child_i;
            
                if (nChildren == 1)
                {
                    child_i = children[firstChild_i].CharIndex == charIndex ? firstChild_i : -1;
                }
                else
                {
                    var searchValue = new YaleChild(-1, charIndex);

                    child_i = Array.BinarySearch(children, firstChild_i, nChildren, searchValue, childComparer);
                }

                return child_i;
            }
        }
            
        return -1;
    }

    public IEnumerable<KeyValuePair<string, int>> MatchTree(IEnumerable<IEnumerable<char>> tree)
    {
        int node_i = rootNodeIndex;

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
                    childIndex = GetChildIndex(node_i, enumerator.Current);

                    if (childIndex >= 0)
                    {
                        break;
                    }
                }
                    
                if (childIndex >= 0)
                {
                    sb.Append(enumerator.Current);
                    stack.Push(new Frame(node_i, enumerator));
                    node_i = children[childIndex].Index;
                    enumerator = null;
                }
                else
                {
                    enumerator.Dispose();
                        
                    if (stack.Count == 0) yield break;
                        
                    (node_i, enumerator) = stack.Pop();
                    --sb.Length;
                }
            }
            else if (stack.Count < enums.Count)
            {
                enumerator = enums[stack.Count].GetEnumerator();
            }
            else
            {
                yield return new KeyValuePair<string, int>(sb.ToString(), node_i);
                    
                if (stack.Count == 0) yield break;
                        
                (node_i, enumerator) = stack.Pop();
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

    public bool IsLeaf(int node_i)
    {
        return firstChildForNode[node_i] == firstChildForNode[node_i + 1];
    }
        
    public IEnumerable<int> MatchPrefix (StringBuilder sb, int node_i)
    {
        if (node_i != -1)
        {
            yield return node_i;
                
            int firstChild_i = firstChildForNode [node_i];

            int lastChild_i = firstChildForNode[node_i + 1];

            for (int i = firstChild_i; i < lastChild_i; ++i)
            {
                YaleChild child = children [i];

                sb.Append (indexToChar [child.CharIndex]);

                foreach (var child_node_i in MatchPrefix (sb, child.Index))
                {
                    yield return child_node_i;
                }

                --sb.Length;
            }
        }
    }
}