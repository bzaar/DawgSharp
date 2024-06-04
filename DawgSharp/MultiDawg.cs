using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawgSharp;

public class MultiDawg<TPayload>
{
    private readonly TPayload[][] payloads;
    private readonly YaleGraph yaleGraph;

    internal MultiDawg(YaleGraph yaleGraph, TPayload[][] payloads)
    {
        this.yaleGraph = yaleGraph;
        this.payloads = payloads;
    }

    /// <summary>
    /// Tries to find as many space-separated words as it can.
    /// </summary>
    /// <param name="words">The words to find</param>
    /// <param name="wordsFound">How many words were actually matched.</param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public IEnumerable<TPayload> MultiwordFind(
        IEnumerable<IEnumerable<char>> words,
        out int wordsFound,
        char separator = ' ')
    {
        int lastWordEndNodeIndex = -1;
        int lastWordCount = 0;
        int nodeIndex = -1;
        int wordCount = 0;
            
        // ReSharper disable AccessToModifiedClosure
        IEnumerable<char> GetChars()
        {
            foreach (IEnumerable<char> word in words)
            {
                foreach (char c in word)
                {
                    yield return c;
                }

                ++wordCount;

                if (HasPayload(nodeIndex))
                {
                    lastWordEndNodeIndex = nodeIndex;
                    lastWordCount = wordCount;
                }
                    
                if (yaleGraph.IsLeaf(nodeIndex))
                {
                    break;
                }
                        
                yield return separator;
            }
        }
        // ReSharper restore AccessToModifiedClosure

        foreach(int i in yaleGraph.GetPath(GetChars()))
        {
            nodeIndex = i;
        }

        wordsFound = lastWordCount;
            
        return GetPayloads(lastWordEndNodeIndex);
    }

    public IEnumerable<TPayload> this[IEnumerable<char> key] => 
        GetPayloads(yaleGraph.GetPath(key).Last());

    private IEnumerable<TPayload> GetPayloads(int i)
    {
        if (i == -1) yield break;
            
        foreach (TPayload[] arr in payloads)
        {
            if (arr.Length <= i)
                break;

            yield return arr[i];
        }
    }

    public IEnumerable<KeyValuePair<string, IEnumerable<TPayload>>> MatchPrefix(IEnumerable<char> prefix)
    {
        string prefixStr = prefix.AsString();

        var sb = new StringBuilder(prefixStr);

        foreach (int nodeIndex in yaleGraph.MatchPrefix(sb, yaleGraph.GetPath(prefixStr).Last()))
        {
            if (HasPayload(nodeIndex))
            {
                yield return new KeyValuePair<string, IEnumerable<TPayload>>(sb.ToString(), GetPayloads(nodeIndex));
            }
        }
    }

    private bool HasPayload(int nodeIndex) => payloads.Length > 0 && nodeIndex < payloads[0].Length;

    public int GetNodeCount() => yaleGraph.NodeCount;

    public int MaxPayloads => payloads.Length;

    public IEnumerable<KeyValuePair<string, IEnumerable<TPayload>>> MatchTree(IEnumerable<IEnumerable<char>> tree)
    {
        return yaleGraph.MatchTree(tree)
            .Select(pair => new KeyValuePair<string, IEnumerable<TPayload>> (pair.Key, GetPayloads(pair.Value)));
    }
}