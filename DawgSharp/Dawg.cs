using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DawgSharp
{
    public class Dawg <TPayload>
    {
        readonly Node <TPayload> root = new Node <TPayload> ();

        public TPayload this [string word]
        {
            get 
            {
                var node = root;

                foreach (char c in word)
                {
                    node = node.GetChild (c);

                    if (node == null) return default (TPayload);
                }

                return node.Payload;
            }
        }

        public static Dawg <TPayload> Load (Stream stream, Func <BinaryReader, TPayload> readPayload)
        {
            using (var reader = new BinaryReader (stream))
            {
                int nodeCount = reader.ReadInt32 ();

                var nodes = new Node <TPayload> [nodeCount];

                var rootIndex = reader.ReadInt32 ();

                var chars = reader.ReadChars (nodeCount);

                for (int i = 0; i < nodes.Length; ++i)
                {
                    var node = new Node <TPayload> ();
                    
                    int childCount = reader.ReadInt16 ();

                    while (childCount --> 0)
                    {
                        int childIndex = reader.ReadInt32 ();

                        node.Children.Add (chars [childIndex], nodes [childIndex]);
                    }

                    node.Payload = readPayload (reader);

                    nodes [i] = node;
                }

                return new Dawg <TPayload> (nodes [rootIndex]);
            }
        }

        class NodeSuperEqualityComparerByNode : IEqualityComparer<NodeWrapper<TPayload>>
        {
            bool IEqualityComparer<NodeWrapper<TPayload>>.Equals(NodeWrapper<TPayload> x, NodeWrapper<TPayload> y)
            {
                return x.Node == y.Node;
            }

            int IEqualityComparer<NodeWrapper<TPayload>>.GetHashCode(NodeWrapper<TPayload> obj)
            {
                return obj.Node.GetHashCode ();
            }
        }

        public void SaveTo (Stream stream, Action <BinaryWriter, TPayload> writePayload)
        {
            var levelBuilder = new LevelBuilder<TPayload> ();

            levelBuilder.GetLevel (root, null, ' ');

            var allNodes = levelBuilder.Levels.SelectMany (level => level.Distinct (new NodeSuperEqualityComparerByNode ()))
                .Concat (new [] {new NodeWrapper<TPayload> (root, null, ' ')})
                .ToArray ();

            var nodeIndex = allNodes
                .Select ((n, i) => new KeyValuePair<Node <TPayload>, int> (n.Node, i))
                .ToDictionary (kvp => kvp.Key, kvp => kvp.Value);

            using (var writer = new BinaryWriter (stream))
            {
                writer.Write (nodeIndex.Count);

                writer.Write (nodeIndex [root]);

                foreach (var nodeSuper in allNodes)
                {
                    writer.Write (nodeSuper.Char);
                }

                foreach (var nodeI in nodeIndex)
                {
                    var node = nodeI.Key;

                    writer.Write (checked ((ushort) node.Children.Count));

                    foreach (var child in node.Children.Values)
                    {
                        writer.Write (nodeIndex [child]);
                    }

                    writePayload (writer, node.Payload);
                }
            }
        }

        internal Dawg (Node <TPayload> root)
        {
            this.root = root;
        }

        public int GetNodeCount ()
        {
            return root.GetChildNodeCount ();
        }
    }
}