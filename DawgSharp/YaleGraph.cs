using System;
using System.Collections.Generic;
using System.Linq;

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

        public YaleGraph(YaleChild[] children,
            int[] firstChildForNode,
            ushort[] charToIndexPlusOne,
            int rootNodeIndex,
            char[] indexToChar)
        {
            this.children = children;
            this.firstChildForNode = firstChildForNode;
            this.charToIndexPlusOne = charToIndexPlusOne;
            this.lastChar = indexToChar.LastOrDefault();
            this.firstChar = indexToChar.FirstOrDefault();
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
    }
}