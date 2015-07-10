using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    public class DawgBuilder <TPayload>
    {
        readonly Node <TPayload> root = new Node <TPayload> ();

        /// <summary>
        /// Inserts a new key/value pair or updates the value for an existing key.
        /// </summary>
        public void Insert (IEnumerable<char> key, TPayload value)
        {
            var node = root;

            foreach (char c in key)
            {
                node = node.GetOrAddEdge (c);
            }

            node.Payload = value;
        }

        public bool TryGetValue (IEnumerable<char> key, out TPayload value)
        {
            value = default (TPayload);

            var node = root;

            foreach (char c in key)
            {
                node = node.GetChild (c);

                if (node == null) return false;
            }

            value = node.Payload;

            return true;
        }

        public Dawg <TPayload> BuildDawg ()
        {
            var levels = LevelBuilder <TPayload>.BuildLevels (root);

            foreach (var level in levels)
            {
                foreach (var similarNodes in level.GroupBy (n => n, n => n, new NodeWrapperEqualityComparer <TPayload> ()))
                {
                    foreach (var similarNode in similarNodes)
                    {
                        similarNode.Super.Children [similarNode.Char] = similarNodes.Key.Node;
                    }
                }
            }

            return new Dawg <TPayload> (root);
        }
    }
}