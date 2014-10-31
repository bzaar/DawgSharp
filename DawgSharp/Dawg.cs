using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DawgSharp
{
    public class Dawg <TPayload> : IEnumerable <KeyValuePair <string, TPayload>>
    {
        readonly Node <TPayload> root = new Node <TPayload> ();

        public TPayload this [IEnumerable<char> word]
        {
            get
            {
                var node = FindNode (word);

                return node == null ? default (TPayload) : node.Payload;
            }
        }

        private Node <TPayload> FindNode (IEnumerable<char> word)
        {
            var node = root;

            foreach (char c in word)
            {
                node = node.GetChild (c);

                if (node == null) return null;
            }

            return node;
        }

        public int GetLongestCommonPrefixLength (IEnumerable<char> word)
        {
            var node = root;
            int len = 0; 

            foreach (char c in word)
            {
                node = node.GetChild (c);

                if (node == null) break;

                ++len;
            }

            return len;
        }

        /// <summary>
        /// Returns all elements with key matching given <paramref name="prefix"/>.
        /// </summary>
        public IEnumerable <KeyValuePair <string, TPayload>> MatchPrefix (IEnumerable<char> prefix)
        {
            var node = FindNode (prefix);

            if (node == null) return Enumerable.Empty <KeyValuePair <string, TPayload>> ();

            var sb = new StringBuilder ();

            sb.Append (prefix.ToArray ());

            return new PrefixMatcher <TPayload> (sb).MatchPrefix (node);
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
            var levels = LevelBuilder <TPayload>.BuildLevels (root);

            var allNodes = levels.SelectMany (level => level.Distinct (new NodeSuperEqualityComparerByNode ()))
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

        public IEnumerator<KeyValuePair<string, TPayload>> GetEnumerator()
        {
            return MatchPrefix ("").GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }
    }
}