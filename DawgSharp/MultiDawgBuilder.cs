using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DawgSharp
{
    public class MultiDawgBuilder<TPayload> : DawgBuilder<IList<TPayload>>
    {
        private const int Version = 0;
        private static readonly int Signature = BitConverter.ToInt32(Encoding.UTF8.GetBytes("MDWG"), 0);

        public MultiDawg<TPayload> BuildMultiDawg()
        {
            FuseEnds();

            var stream = new MemoryStream();
            SaveTo(stream);
            stream.Position = 0;

            return LoadFrom(stream);
        }

        public void FuseEnds()
        {
            new LevelBuilder<IList<TPayload>>(new SequenceEqualityComparer<TPayload>()).MergeEnds(root);
        }

        public static MultiDawg<TPayload> LoadFrom(MemoryStream stream)
        {
            if (!BuiltinTypeIO.Readers.TryGetValue(typeof(TPayload), out object payloadReader))
            {
                throw new Exception($"No built in reader found for type {nameof(TPayload)}.");
            }

            return LoadFrom(stream, (Func<BinaryReader, TPayload>)payloadReader);
        }

        public static MultiDawg<TPayload> LoadFrom(MemoryStream stream, Func <BinaryReader, TPayload> readPayload)
        {
            var reader = new BinaryReader(stream);
            if (reader.ReadInt32() != Signature)
            {
                throw new Exception("Invalid signature.");
            }
            if (reader.ReadInt32() != Version)
            {
                throw new Exception("Invalid version.");
            }
                
            int nodeCount = reader.ReadInt32();
            int rootNodeIndex = reader.ReadInt32();

            TPayload[][] payloads = reader.ReadArray(r => r.ReadArray(readPayload));

            char[] indexToChar = reader.ReadArray(r => r.ReadChar());

            ushort[] charToIndexPlusOne = CharToIndexPlusOneMap.Get(indexToChar);

            YaleReader.ReadChildren(indexToChar, nodeCount, reader, out var firstChildForNode, out var children);
            var yaleGraph = new YaleGraph(children, firstChildForNode, charToIndexPlusOne, rootNodeIndex, indexToChar);
            return new MultiDawg<TPayload>(yaleGraph, payloads);
        }

        public void SaveTo(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Signature);
            writer.Write(Version);
            Serializer.SaveAsMultiDawg(writer, root, Serializer.GetStandardWriter<TPayload>());
            writer.Flush(); // do not close the stream
        }
    }
}