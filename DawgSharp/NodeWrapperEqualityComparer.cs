using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class NodeWrapperEqualityComparer <TPayload> : IEqualityComparer <NodeWrapper <TPayload>>
    {
        public bool Equals (NodeWrapper <TPayload> x, NodeWrapper <TPayload> y)
        {
            var @equals = x.Char == y.Char
                          && x.Node.Payload.Equals (y.Node.Payload) 
                          && x.Node.SortedChildren.SequenceEqual (y.Node.SortedChildren);

            return @equals;
        }

        public int GetHashCode (NodeWrapper <TPayload> obj)
        {
            var hashCode = obj.Node.Payload.GetHashCode () + obj.Node.SortedChildren.Sum (c => c.GetHashCode ()) + obj.Char.GetHashCode ();

            return hashCode;
        }
    }
}