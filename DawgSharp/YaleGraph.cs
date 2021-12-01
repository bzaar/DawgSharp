using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawgSharp
{
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

            if (node_i == -1)
            {
                yield return -1;
                yield break;
            }

            yield return node_i;

            foreach (char c in word)
            {
                ushort charIndexPlusOne;

                if (c < firstChar || c > lastChar || (charIndexPlusOne = charToIndexPlusOne[c - firstChar]) == 0)
                {
                    yield return -1;
                    yield break;
                }

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

                if (child_i < 0)
                {
                    yield return -1;
                    yield break;
                }

                node_i = children[child_i].Index;

                yield return node_i;
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
}