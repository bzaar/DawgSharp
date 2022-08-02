using System;
using System.Collections.Generic;

namespace DawgSharp;

public static class DawgExtensions
{
    public static Dawg<TPayload> ToDawg<T, TPayload>(this IEnumerable<T> enumerable, Func<T, IEnumerable<char>> key,
        Func<T, TPayload> payload)
    {
        var dawgBuilder = ToDawgBuilder(enumerable, key, payload);

        return dawgBuilder.BuildDawg();
    }

    public static DawgBuilder<TPayload> ToDawgBuilder<T, TPayload>(this IEnumerable<T> enumerable,
        Func<T, IEnumerable<char>> key, Func<T, TPayload> payload)
    {
        var dawgBuilder = new DawgBuilder<TPayload>();

        foreach (T elem in enumerable)
        {
            dawgBuilder.Insert(key(elem), payload(elem));
        }

        return dawgBuilder;
    }
}