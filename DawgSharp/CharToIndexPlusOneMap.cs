namespace DawgSharp;

static class CharToIndexPlusOneMap
{
    public static ushort[] Get(char [] uniqueChars)
    {
        if (uniqueChars.Length == 0) return null;

        var charToIndex = new ushort [uniqueChars.Last() - uniqueChars.First() + 1];

        for (int i = 0; i < uniqueChars.Length; ++i)
        {
            charToIndex [uniqueChars [i] - uniqueChars.First()] = (ushort) (i + 1);
        }

        return charToIndex;
    }
}