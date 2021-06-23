using System.Collections.Generic;
using System.IO;

namespace DawgSharp
{
    public class WildDawg<TPayload>
    {
        private readonly YaleDawg<TPayload> yaleDawg;

        WildDawg(YaleDawg<TPayload> yaleDawg)
        {
            this.yaleDawg = yaleDawg;
        }

        public TPayload this[IEnumerable<char> word] =>
            yaleDawg.GetPayloadUsingWildcards(word);

        public static WildDawg<TPayload> LoadFrom(Stream stream)
        {
            var binaryReader = new BinaryReader(stream);

            binaryReader.ReadInt32(); // signature
            binaryReader.ReadInt32(); // version (2)

            return new(new YaleDawg<TPayload>(binaryReader, Dawg<TPayload>.GetBuiltInTypeReader()));
        }
    }
}