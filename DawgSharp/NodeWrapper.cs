using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class NodeWrapper <TPayload>
    {
        public NodeWrapper (Node<TPayload> node)
        {
            this.Node = node;
        }

        public readonly Node<TPayload> Node;
        KeyValuePair<char, Node<TPayload>> [] sortedChildren;
        public int? HashCode; // set by the comparer, cached for efficiency

        private KeyValuePair<char, Node<TPayload>>[] GetSortedChildren()
        {
            return Node.Children.OrderBy(c => c.Key).ToArray();
        }

        public KeyValuePair<char, Node<TPayload>> [] SortedChildren => sortedChildren ?? (sortedChildren = GetSortedChildren());
    }
}