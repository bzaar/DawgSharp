using System.Collections.Generic;

namespace DawgSharp;

class LevelBuilderEqualityComparer <TPayload> : IEqualityComparer <Node<TPayload>>
{
    private readonly IEqualityComparer<TPayload> payloadComparer;

    public LevelBuilderEqualityComparer(IEqualityComparer<TPayload> payloadComparer)
    {
        this.payloadComparer = payloadComparer;
    }
        
    public bool Equals (Node<TPayload> x, Node<TPayload> y)
    {
        // ReSharper disable PossibleNullReferenceException
        bool equals = AreEqual(x, y);
        // ReSharper restore PossibleNullReferenceException

        return equals;
    }

    private bool AreEqual(Node<TPayload> xNode, Node<TPayload> yNode)
    {
        bool equals = payloadComparer.Equals(xNode.Payload, yNode.Payload)
                      && SequenceEqual(xNode.SortedChildren, yNode.SortedChildren);
        return equals;
    }

    private bool SequenceEqual(
        IEnumerable<KeyValuePair<char, Node<TPayload>>> x, 
        IEnumerable<KeyValuePair<char, Node<TPayload>>> y)
    {
        // Do not bother disposing of these enumerators.

        // ReSharper disable GenericEnumeratorNotDisposed
        var xe = x.GetEnumerator();
        var ye = y.GetEnumerator();
        // ReSharper restore GenericEnumeratorNotDisposed

        while (xe.MoveNext())
        {
            if (!ye.MoveNext()) return false;

            var xcurrent = xe.Current;
            var ycurrent = ye.Current;

            if (xcurrent.Key != ycurrent.Key) return false;
            
            // Child nodes have already been merged 
            // so we can use reference equality here.
            if (xcurrent.Value != ycurrent.Value) return false;
        }

        return !ye.MoveNext();
    }

    private int ComputeHashCode(Node<TPayload> node)
    {
        int hashCode = payloadComparer.GetHashCode(node.Payload);

        foreach (var pair in node.Children)
        {
            char c = pair.Key;
            Node<TPayload> childNode = pair.Value;

            // Child nodes have already been merged 
            // so we can use reference equality here.
            hashCode ^= c ^ childNode.GetHashCode();
        }

        return hashCode;
    }

    public int GetHashCode (Node<TPayload> node)
    {
        return ComputeHashCode(node);
    }
}