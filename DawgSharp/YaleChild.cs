namespace DawgSharp;

readonly struct YaleChild
{
    public readonly int Index;
    public readonly ushort CharIndex;

    public YaleChild(int index, ushort charIndex)
    {
        Index = index;
        CharIndex = charIndex;
    }
}