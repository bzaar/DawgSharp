namespace DawgSharp;

static class YaleReader
{
    public static void ReadChildren(char[] indexToChar, int nodeCount, BinaryReader reader,
        out int[] firstChildForNode,
        out YaleChild[] children)
    {
        firstChildForNode = new int[nodeCount + 1];

        int firstChildForNode_i = 0;

        int totalChildCount = reader.ReadInt32();

        children = new YaleChild [totalChildCount];

        firstChildForNode[nodeCount] = totalChildCount;

        int globalChild_i = 0;

        for (int child1_i = 0; child1_i < nodeCount; ++child1_i)
        {
            firstChildForNode[firstChildForNode_i++] = globalChild_i;

            ushort childCount = ReadInt(reader, indexToChar.Length + 1);

            for (ushort child_i = 0; child_i < childCount; ++child_i)
            {
                ushort charIndex = ReadInt(reader, indexToChar.Length);
                int childNodeIndex = reader.ReadInt32();

                children[globalChild_i++] = new YaleChild(childNodeIndex, charIndex);
            }
        }
    }

    public static ushort ReadInt (BinaryReader reader, int countOfPossibleValues)
    {
        return countOfPossibleValues > 256 ? reader.ReadUInt16() : reader.ReadByte();
    }
}