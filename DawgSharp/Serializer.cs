using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DawgSharp;

static class Serializer
{
    public static void SaveAsYaleDawg<TPayload>(this Node<TPayload> root, BinaryWriter writer, Action<BinaryWriter, TPayload> writePayload)
    {
        const int version = 2;
        writer.Write (version);

        Node<TPayload>[] allNodes = root.GetAllDistinctNodes().ToArray();

        Array.Sort(allNodes, new NodeByPayloadComparer<TPayload>());
        int totalChildCount = allNodes.Sum(n => n.Children.Count);

        writer.Write (allNodes.Length);

        var nodeIndex = allNodes
            .Select((node, i) => new {node, i})
            .ToDictionary(t => t.node, t => t.i);

        if (!nodeIndex.TryGetValue(root, out int rootNodeIndex))
        {
            rootNodeIndex = -1;
        }

        writer.Write (rootNodeIndex);

        Node<TPayload>[] nodesWithPayloads = allNodes.TakeWhile(n => n.HasPayload).ToArray();

        writer.WriteArray(nodesWithPayloads, (binaryWriter, node) => writePayload(binaryWriter, node.Payload));

        WriteCharsAndChildren(writer, allNodes, totalChildCount, nodeIndex);
    }

    private static void WriteCharsAndChildren<T>(BinaryWriter writer, Node<T>[] allNodes, int totalChildCount,
        Dictionary<Node<T>, int> nodeIndex)
    {
        var allChars = allNodes.SelectMany(node => node.Children.Keys).Distinct().OrderBy(c => c).ToArray();

        writer.WriteArray(allChars);

        writer.Write(totalChildCount);

        WriteChildrenNoLength(writer, allNodes, nodeIndex, allChars);
    }

    public static void SaveAsMultiDawg<TPayload>(BinaryWriter writer, Node<IList<TPayload>> root, Action<BinaryWriter, TPayload> writePayload)
    {
        var groups = root
            .GetAllDistinctNodes()
            .GroupBy(n => n.Payload?.Count ?? 0)
            .OrderByDescending(g => g.Key)
            .Select(g => new {PayloadCount = g.Key, Nodes = g.ToList()})
            .ToList();
            
        Node<IList<TPayload>>[] allNodes = groups
            .SelectMany(g => g.Nodes)
            .ToArray();

        int totalChildCount = allNodes.Sum(n => n.Children.Count);

        writer.Write (allNodes.Length);

        var nodeIndex = allNodes
            .Select((node, i) => new {node, i})
            .ToDictionary(t => t.node, t => t.i);

        if (!nodeIndex.TryGetValue(root, out int rootNodeIndex))
        {
            rootNodeIndex = -1;
        }

        writer.Write (rootNodeIndex);

        int maxPayloadCount = groups.FirstOrDefault()?.PayloadCount ?? 0;

        writer.Write(maxPayloadCount);
            
        for (int payload_i = 0; payload_i < maxPayloadCount; ++payload_i)
        {
            var ggs = groups.TakeWhile(g => g.PayloadCount > payload_i).ToList();
                
            writer.Write(ggs.Sum(g => g.Nodes.Count));

            foreach (var n in ggs.SelectMany(g => g.Nodes))
            {
                writePayload(writer, n.Payload[payload_i]);
            }
        }

        WriteCharsAndChildren(writer, allNodes, totalChildCount, nodeIndex);
    }
        
    public static void SaveAsMatrixDawg<TPayload>(this Node<TPayload> root, BinaryWriter writer, Action<BinaryWriter, TPayload> writePayload)
    {
        const int version = 1;
        writer.Write (version);

        var allNodes = root.GetAllDistinctNodes()
            .ToArray();

        writer.Write (allNodes.Length);

        var cube = new Node<TPayload> [2,2] [];

        var nodeGroups = allNodes.GroupBy (node => new {node.HasPayload, node.HasChildren})
            .ToDictionary(g => g.Key, g => g.ToArray());

        for (int p = 0; p < 2; ++p)
        for (int c = 0; c < 2; ++c)
        {
            var key = new {HasPayload = p != 0, HasChildren = c != 0};
            cube [p, c] = nodeGroups.TryGetValue(key, out var arr) ? arr : new Node<TPayload>[0];
        }

        var nodesWithPayloads = cube [1, 1].Concat(cube [1, 0]).ToArray();

        var nodeIndex = nodesWithPayloads.Concat(cube [0, 1].Concat(cube [0, 0]))
            .Select((node, i) => new {node, i})
            .ToDictionary(t => t.node, t => t.i);

        var rootNodeIndex = nodeIndex [root];

        writer.Write (rootNodeIndex);

        writer.Write (nodesWithPayloads.Length);

        foreach (var node in nodesWithPayloads)
        {
            writePayload (writer, node.Payload);
        }

        var allChars = allNodes.SelectMany (node => node.Children.Keys).Distinct().OrderBy(c => c).ToArray();

        writer.Write (allChars.Length);

        foreach (char c in allChars)
        {
            writer.Write (c);
        }

        WriteChildren (writer, nodeIndex, cube [1, 1], allChars);
        WriteChildren (writer, nodeIndex, cube [0, 1], allChars);
    }

    private static void WriteChildren<TPayload>(BinaryWriter writer, Dictionary<Node<TPayload>, int> nodeIndex, Node<TPayload>[] nodes, char[] allChars)
    {
        writer.Write (nodes.Length);

        WriteChildrenNoLength(writer, nodes, nodeIndex, allChars);
    }

    private static void WriteChildrenNoLength<T>(BinaryWriter writer, IEnumerable<Node<T>> nodes, Dictionary<Node<T>, int> nodeIndex, char[] allChars)
    {
        ushort[] charToIndexPlusOne = CharToIndexPlusOneMap.Get (allChars);

        char firstChar = allChars.FirstOrDefault();

        foreach (var node in nodes)
        {
            WriteInt (writer, node.Children.Count, allChars.Length + 1);

            foreach (var child in node.Children.OrderBy(c => c.Key))
            {
                int charIndex = charToIndexPlusOne [child.Key - firstChar] - 1;

                WriteInt (writer, charIndex, allChars.Length);

                writer.Write (nodeIndex [child.Value]);
            }
        }
    }

    private static void WriteInt(BinaryWriter writer, int charIndex, int numPossibleValues)
    {
        if (numPossibleValues > 256)
        {
            writer.Write ((ushort) charIndex);
        }
        else
        {
            writer.Write ((byte) charIndex);
        }
    }
}