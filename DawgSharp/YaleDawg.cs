using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DawgSharp
{
    class YaleDawg <TPayload> : IDawg<TPayload>
    {
        private readonly TPayload[] payloads;
        private readonly char[] indexToChar;
        private readonly int nodeCount, rootNodeIndex;
        private readonly int [] firstChildForNode;
        private readonly YaleChild[] children;
        private readonly YaleGraph yaleGraph;

        public YaleDawg (BinaryReader reader, Func <BinaryReader, TPayload> readPayload)
        {
            nodeCount = reader.ReadInt32();

            rootNodeIndex = reader.ReadInt32();

            payloads = reader.ReadArray(readPayload);

            indexToChar = reader.ReadArray(r => r.ReadChar());

            ushort[] charToIndexPlusOne = CharToIndexPlusOneMap.Get(indexToChar);

            YaleReader.ReadChildren(indexToChar, nodeCount, reader, out firstChildForNode, out children);

            yaleGraph = new YaleGraph(children, firstChildForNode, charToIndexPlusOne, rootNodeIndex, indexToChar);
        }

        TPayload IDawg<TPayload>.this[IEnumerable<char> word]
        {
            get
            {
                int node_i = GetPath(word).Last();

                if (node_i == -1) return default;

                return GetPayload(node_i);
            }
        }

        private TPayload GetPayload(int node_i)
        {
            return node_i < payloads.Length ? payloads [node_i] : default;
        }

        IEnumerable<int> GetPath(IEnumerable<char> word)
        {
            return yaleGraph.GetPath(word);
        }

        int IDawg<TPayload>.GetLongestCommonPrefixLength(IEnumerable<char> word)
        {
            return GetPath (word).Count(i => i != -1) - 1; // -1 for root node
        }

        IEnumerable<KeyValuePair<string, TPayload>> IDawg<TPayload>.MatchPrefix(IEnumerable<char> prefix)
        {
            string prefixStr = prefix.AsString();

            int node_i = GetPath (prefixStr).Last();

            var sb = new StringBuilder (prefixStr);

            return MatchPrefix(sb, node_i);
        }

        public IEnumerable<KeyValuePair<string, TPayload>> GetPrefixes(IEnumerable<char> key)
        {
            var sb = new StringBuilder();

            string keyStr = key.AsString();

            int strIndex = -1;

            foreach (int node_i in GetPath(keyStr))
            {
                if (node_i == -1) break;

                if (strIndex >= 0) sb.Append(keyStr[strIndex]);

                var payload = GetPayload(node_i);
                
                if (!EqualityComparer<TPayload>.Default.Equals(payload, default))
                {
                    yield return new KeyValuePair<string, TPayload>(sb.ToString(), payload);
                }

                if (strIndex++ == keyStr.Length) break;
            }
        }

        private IEnumerable<KeyValuePair<string, TPayload>> MatchPrefix (StringBuilder sb, int node_i)
        {
            if (node_i != -1)
            {
                var payload = GetPayload(node_i);

                if (!EqualityComparer<TPayload>.Default.Equals(payload, default))
                {
                    yield return new KeyValuePair<string, TPayload>(sb.ToString(), payload);
                }

                int firstChild_i = firstChildForNode [node_i];

                int lastChild_i = node_i + 1 < nodeCount
                                        ? firstChildForNode[node_i + 1]
                                        : children.Length;

                for (int i = firstChild_i; i < lastChild_i; ++i)
                {
                    YaleChild child = children [i];

                    sb.Append (indexToChar [child.CharIndex]);

                    foreach (var pair in MatchPrefix (sb, child.Index))
                    {
                        yield return pair;
                    }

                    --sb.Length;
                }
            }
        }

        int IDawg<TPayload>.GetNodeCount()
        {
            return nodeCount;
        }

        public KeyValuePair<string, TPayload> GetRandomItem(Random random)
        {
            int nodeIndex = random.Next(0, payloads.Length);

            TPayload payload = payloads[nodeIndex];

            var sb = new StringBuilder();
            
            for (;;)
            {
                var childIndexes = children.Select((c, i) => new {c, i})
                    .Where(t => t.c.Index == nodeIndex)
                    .Select(t => t.i)
                    .ToList();

                int childIndex = childIndexes[random.Next(0, childIndexes.Count)];

                sb.Insert(0, indexToChar[children[childIndex].CharIndex]);
                
                int parentIndex = Array.BinarySearch(firstChildForNode, childIndex);

                if (parentIndex < 0)
                {
                    parentIndex = ~parentIndex - 1;
                }
                else
                {
                    while (parentIndex < firstChildForNode.Length - 1 && firstChildForNode[parentIndex + 1] == childIndex) 
                        ++parentIndex;
                }

                if (parentIndex == rootNodeIndex)
                {
                    return new KeyValuePair<string, TPayload>(sb.ToString(), payload);                    
                }

                nodeIndex = parentIndex;
            }
        }
    }
}