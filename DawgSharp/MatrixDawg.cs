using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DawgSharp;

class MatrixDawg <TPayload> : IDawg <TPayload>
{
    public TPayload this [IEnumerable <char> word]
    {
        get
        {
            int nodeIndex = rootNodeIndex;

            foreach (var c in word)
            {
                int childIndexPlusOne = GetChildIndexPlusOne(nodeIndex, c);

                if (childIndexPlusOne == 0) return default;

                nodeIndex = childIndexPlusOne - 1;
            }

            if (nodeIndex == -1) return default;

            return nodeIndex < payloads.Length ? payloads [nodeIndex] : default;
        }
    }

    /// <summary>
    /// Returns a series of node indices 
    /// </summary>
    IEnumerable <int> GetPath (IEnumerable<char> word)
    {
        int nodeIndex = rootNodeIndex;

        yield return nodeIndex;

        foreach (var c in word)
        {
            int childIndexPlusOne = GetChildIndexPlusOne(nodeIndex, c);

            if (childIndexPlusOne == 0)
            {
                yield return -1;
                yield break;
            }

            nodeIndex = childIndexPlusOne - 1;

            yield return nodeIndex;
        }
    }

    int GetChildIndexPlusOne (int nodeIndex, char c)
    {
        var children = nodeIndex < payloads.Length ? children1 : children0;

        if (nodeIndex >= payloads.Length) nodeIndex -= payloads.Length;

        if (nodeIndex >= children.GetLength(0)) return 0; // node has no children

        if (c < firstChar) return 0;
        if (c > lastChar) return 0;

        ushort charIndexPlusOne = charToIndexPlusOne [c - firstChar];

        if (charIndexPlusOne == 0) return 0;

        return children [nodeIndex, charIndexPlusOne - 1];
    }

    public int GetLongestCommonPrefixLength (IEnumerable <char> word)
    {
        return GetPath (word).Count(i => i != -1) - 1;
    }

    struct StackItem
    {
        public int NodeIndex, ChildIndex;
    }

    public IEnumerable <KeyValuePair <string, TPayload>> MatchPrefix (IEnumerable<char> prefix)
    {
        string prefixStr = prefix.AsString();

        int nodeIndex = prefixStr.Length == 0 ? rootNodeIndex : GetPath (prefixStr).Last();

        var stack = new Stack<StackItem>();

        if (nodeIndex != -1)
        {
            if (nodeIndex < payloads.Length)
            {
                var payload = payloads [nodeIndex];

                if (! EqualityComparer<TPayload>.Default.Equals(payload, default))
                {
                    yield return new KeyValuePair<string, TPayload> (prefixStr, payload);
                }
            }

            var sb = new StringBuilder (prefixStr);

            int childIndex = -1;

            for (;;)
            {
                var children = nodeIndex < payloads.Length ? children1 : children0;

                int adj_nodeIndex = (nodeIndex >= payloads.Length) 
                    ? nodeIndex - payloads.Length
                    : nodeIndex;

                if (adj_nodeIndex < children.GetLength(0))
                {
                    int next_childIndex = childIndex + 1;

                    for (; next_childIndex < indexToChar.Length; ++next_childIndex)
                    {
                        if (children [adj_nodeIndex, next_childIndex] != 0)
                        {
                            break;
                        }
                    }

                    if (next_childIndex < indexToChar.Length)
                    {
                        stack.Push(new StackItem {NodeIndex = nodeIndex, ChildIndex = next_childIndex});
                        sb.Append(indexToChar [next_childIndex]);
                        nodeIndex = children [adj_nodeIndex, next_childIndex] - 1;

                        if (nodeIndex < payloads.Length)
                        {
                            var payload = payloads [nodeIndex];

                            if (! EqualityComparer<TPayload>.Default.Equals(payload, default))
                            {
                                yield return new KeyValuePair<string, TPayload> (sb.ToString(), payload);
                            }
                        }

                        continue;
                    }
                }

                // No (more) children.

                if (stack.Count == 0) break;

                --sb.Length;
                var item = stack.Pop();

                nodeIndex = item.NodeIndex;
                childIndex = item.ChildIndex;
            }
        }
    }

    IEnumerable<KeyValuePair<string, TPayload>> IDawg<TPayload>.GetPrefixes(IEnumerable<char> key)
    {
        throw new NotImplementedException();
    }

    public void SaveAsOldDawg (Stream stream, Action <BinaryWriter, TPayload> writePayload)
    {
        throw new NotImplementedException();
    }

    public int GetNodeCount()
    {
        return nodeCount;
    }

    public KeyValuePair<string, TPayload> GetRandomItem(Random random)
    {
        throw new NotImplementedException();
    }

    private readonly TPayload[] payloads;
    private readonly int[,] children1;
    private readonly int[,] children0;
    private readonly char[] indexToChar;
    private readonly ushort[] charToIndexPlusOne;
    private readonly int nodeCount, rootNodeIndex;
    private readonly char firstChar;
    private readonly char lastChar;

    public MatrixDawg (BinaryReader reader, Func <BinaryReader, TPayload> readPayload)
    {
        // The nodes are grouped by (has payload, has children).
        nodeCount = reader.ReadInt32();

        rootNodeIndex = reader.ReadInt32();

        payloads = reader.ReadArray(readPayload);

        indexToChar = reader.ReadArray(r => r.ReadChar());

        charToIndexPlusOne = CharToIndexPlusOneMap.Get(indexToChar);

        children1 = ReadChildren(reader, indexToChar);
        children0 = ReadChildren(reader, indexToChar);

        firstChar = indexToChar.FirstOrDefault();
        lastChar = indexToChar.LastOrDefault();
    }

    private static int[,] ReadChildren(BinaryReader reader, char[] indexToChar)
    {
        uint nodeCount = reader.ReadUInt32();

        var children = new int [nodeCount, indexToChar.Length];

        for (int nodeIndex = 0; nodeIndex < nodeCount; ++nodeIndex)
        {
            ushort childCount = YaleReader.ReadInt (reader, indexToChar.Length + 1);

            for (ushort childIndex = 0; childIndex < childCount; ++childIndex)
            {
                ushort charIndex = YaleReader.ReadInt (reader, indexToChar.Length);
                int childNodeIndex = reader.ReadInt32();

                children [nodeIndex, charIndex] = childNodeIndex + 1;
            }
        }

        return children;
    }
}