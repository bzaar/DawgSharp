using System.IO;

namespace DawgSharp;

static class YaleReader
{
    public static void ReadChildren(char[] indexToChar, int nodeCount, BinaryReader reader,
        out int[] firstChildForNode,
        out YaleChild[] children)
    {
        firstChildForNode = new int[nodeCount + 1];

        int firstChildForNodeIndex = 0;

        int totalChildCount = reader.ReadInt32();

        children = new YaleChild [totalChildCount];

        firstChildForNode[nodeCount] = totalChildCount;

        int globalChildIndex = 0;

        for (int child1Index = 0; child1Index < nodeCount; ++child1Index)
        {
            firstChildForNode[firstChildForNodeIndex++] = globalChildIndex;

            ushort childCount = ReadInt(reader, indexToChar.Length + 1);

            for (ushort childIndex = 0; childIndex < childCount; ++childIndex)
            {
                ushort charIndex = ReadInt(reader, indexToChar.Length);
                int childNodeIndex = reader.ReadInt32();

                children[globalChildIndex++] = new YaleChild(childNodeIndex, charIndex);
            }
        }
    }

    public static ushort ReadInt (BinaryReader reader, int countOfPossibleValues)
    {
        return countOfPossibleValues > 256 ? reader.ReadUInt16() : reader.ReadByte();
    }
}