using System;
using System.Collections.Generic;
using System.Linq;

namespace DawgSharp;

public static class DawgExtensionsParallel
{
    public static Dawg <TPayload> ToDawgParallel<T, TPayload>(this IEnumerable <T> enumerable, Func<T, string> key, Func<T, TPayload> payload) => 
        ToDawgBuilderParallel(enumerable, key, payload).BuildDawg ();

    /// <summary>
    /// Adds all the words in the enumerable to a new DawgBuilder.
    /// </summary>
    /// <remarks>
    /// Splits the word list into groups by the first letter of the word
    /// and calls <see cref="DawgBuilder{TPayload}.Insert"/> for each group in parallel.
    /// </remarks>
    public static DawgBuilder<TPayload> ToDawgBuilderParallel<T, TPayload>(
        this IEnumerable<T> enumerable,
        Func<T, string> key,
        Func<T, TPayload> payload)
    {
        return ToDawgBuilderParallel2(enumerable, key, payload, DawgBuilder<TPayload>.Merge);
    }

    public static MultiDawgBuilder<TPayload> ToMultiDawgBuilderParallel<T, TPayload>(
        this IEnumerable<T> enumerable,
        Func<T, string> key,
        Func<T, IList<TPayload>> payload)
    {
        return ToDawgBuilderParallel2(enumerable, key, payload, MultiDawgBuilder<TPayload>.MergeMulti);
    }

    private static TDawgBuilder ToDawgBuilderParallel2<T, TPayload, TDawgBuilder>(
        IEnumerable<T> enumerable,
        Func<T, string> key,
        Func<T, TPayload> payload,
        Func<Dictionary<char, Node<TPayload>>, TDawgBuilder> merge) where TDawgBuilder : DawgBuilder<TPayload>
    {
        var lookup = enumerable.ToLookup(w => key(w).Length > 1);
        var shortKeys = lookup[false];
        var longKeys = lookup[true];

        var dawgBuilders = longKeys
            .GroupBy(item => key(item).First())
            .AsParallel()
            .ToDictionary(g => g.Key, g => g.ToDawgBuilder(item => key(item).Skip(1), payload).root);

        var dawgBuilder = merge(dawgBuilders);

        foreach (T w in shortKeys)
        {
            dawgBuilder.Insert(key(w), payload(w));
        }

        return dawgBuilder;
    }
}