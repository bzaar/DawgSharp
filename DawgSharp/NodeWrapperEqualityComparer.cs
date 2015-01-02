using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class NodeWrapperEqualityComparer <TPayload> : IEqualityComparer <NodeWrapper <TPayload>>
    {
        public bool Equals (NodeWrapper <TPayload> x, NodeWrapper <TPayload> y)
        {
            var @equals = x.Char == y.Char
                          && EqualityComparer<TPayload>.Default.Equals (x.Node.Payload, y.Node.Payload) 
                          && x.Node.SortedChildren.SequenceEqual (y.Node.SortedChildren);

            return @equals;
        }

        public int GetHashCode (NodeWrapper <TPayload> obj)
        {
            var hashCode = EqualityComparer<TPayload>.Default.GetHashCode (obj.Node.Payload) 
                + obj.Node.Children.Select (c => c.Value).Sum (c => c.GetHashCode ()) 
                + obj.Char.GetHashCode ();

            return hashCode;
        }
    }
}