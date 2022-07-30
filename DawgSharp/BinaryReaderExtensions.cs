namespace DawgSharp;

static class BinaryReaderExtensions
{
    public static T [] ReadArray <T> (this BinaryReader reader, Func<BinaryReader, T> read)
    {
        int len = reader.ReadInt32();

        return ReadSequence(reader, read).Take(len).ToArray();
    }

    static IEnumerable<T> ReadSequence <T> (BinaryReader reader, Func<BinaryReader, T> read)
    {
        for (;;) yield return read (reader);
        // ReSharper disable once IteratorNeverReturns
    }
}