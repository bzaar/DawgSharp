namespace DawgSharp;

class OldDawg <TPayload> : IDawg<TPayload>
{
    internal readonly Node<TPayload> root;

    public TPayload this [IEnumerable<char> word]
    {
        get
        {
            var node = FindNode (word);

            return node == null ? default : node.Payload;
        }
    }

    private Node<TPayload> FindNode (IEnumerable<char> word)
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
    public IEnumerable<KeyValuePair<string, TPayload>> MatchPrefix (IEnumerable<char> prefix)
    {
        string prefixStr = prefix.AsString();

        var node = FindNode (prefixStr);

        if (node == null) return Enumerable.Empty <KeyValuePair<string, TPayload>> ();

        var sb = new StringBuilder ();

        sb.Append (prefixStr);

        return new PrefixMatcher<TPayload>(sb).MatchPrefix (node);
    }

    IEnumerable<KeyValuePair<string, TPayload>> IDawg<TPayload>.GetPrefixes(IEnumerable<char> key)
    {
        throw new NotImplementedException();
    }

    internal OldDawg (Node<TPayload> root)
    {
        this.root = root;
    }

    public int GetNodeCount ()
    {
        return root.GetRecursiveChildNodeCount ();
    }

    public KeyValuePair<string, TPayload> GetRandomItem(Random random)
    {
        throw new NotImplementedException();
    }
}

class NodeByPayloadComparer<TPayload> : IComparer<Node<TPayload>>
{
    public int Compare(Node<TPayload> x, Node<TPayload> y)
    {
        // ReSharper disable PossibleNullReferenceException
        return - x.HasPayload.CompareTo(y.HasPayload);
        // ReSharper restore PossibleNullReferenceException
    }
}