namespace DawgSharp
{
    struct NodeWrapper <TPayload>
    {
        public NodeWrapper (Node<TPayload> node, Node<TPayload> super, char @char)
        {
            this.Node = node;
            this.Super = super;
            this.Char = @char;
        }

        public readonly Node<TPayload> Node;
        public readonly Node<TPayload> Super;
        public readonly char Char;
    }
}