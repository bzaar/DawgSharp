using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    public class DawgBuilder <TPayload>
    {
        readonly Node <TPayload> root = new Node <TPayload> ();

        public void Insert (IEnumerable<char> key, TPayload value)
        {
            var node = root;

            foreach (char c in key)
            {
                node = node.GetOrAddEdge (c);
            }

            node.Payload = value;
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