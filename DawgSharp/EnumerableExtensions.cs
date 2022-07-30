using System.Collections.Generic;
using System.Text;

namespace DawgSharp;

static class EnumerableExtensions
{
    public static string AsString(this IEnumerable<char> seq)
    {
        return seq as string ?? ToString(seq);
    }

    static string ToString(IEnumerable<char> seq)
    {
        var sb = new StringBuilder();

        foreach (char c in seq)
        {
            sb.Append(c);
        }

        return sb.ToString();
    }
}