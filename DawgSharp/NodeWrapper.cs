namespace DawgSharp;

class NodeWrapper <TPayload>
{
    public NodeWrapper (Node<TPayload> node)
    {
        this.Node = node;
    }

    public readonly Node<TPayload> Node;
    public int? HashCode; // set by the comparer, cached for efficiency
}