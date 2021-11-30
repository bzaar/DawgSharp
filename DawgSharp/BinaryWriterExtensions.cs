using System;
using System.Collections.Generic;
using System.IO;

namespace DawgSharp
{
    static class BinaryWriterExtensions
    {
        public static void WriteArray<T>(this BinaryWriter writer, ICollection<T> array,
            Action<BinaryWriter, T> writeElement)
        {
            writer.Write(array.Count);

            foreach (T elem in array)
            {
                writeElement(writer, elem);
            }
        }
        
        public static void WriteArray(this BinaryWriter writer, ICollection<char> array)
        {
            writer.WriteArray(array, (binaryWriter, c) => binaryWriter.Write(c));
        }
    }
}