using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class Node <TPayload>
    {
        readonly Dictionary <char, Node <TPayload>> children = new Dictionary <char, Node <TPayload>> ();

        public TPayload Payload { get; set; }

        public Node <TPayload> GetOrAddEdge (char c)
        {
            Node <TPayload> newNode;

            if (! children.TryGetValue (c, out newNode))
            {
                newNode = new Node <TPayload> ();

                children.Add (c, newNode);
            }

            return newNode;
        }

        public Node <TPayload> GetChild (char c)
        {
            Node <TPayload> node;

            children.TryGetValue (c, out node);

            return node;
        }

        public bool HasChildren ()
        {
            return children.Count > 0;
        }

        public Dictionary <char, Node<TPayload>> Children
        {
            get {return children;}
        }

        public IEnumerable<Node <TPayload>> SortedChildren
        {
            get {return children.OrderBy (e => e.Key).Select (e => e.Value);}
        }

        public int GetChildNodeCount ()
        {
            return GetAllNodes ().Distinct ().Count ();
        }

        public IEnumerable<Node<TPayload>> GetAllNodes ()
        {
            return new [] {this}.Concat (children.Values.SelectMany (node => node.GetAllNodes ()));
        }
    }
}