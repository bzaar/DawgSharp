using System.IO;
using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class YaleDawgTests : DawgTests
    {
        protected override Dawg<TPayload> GetDawg <TPayload> (DawgBuilder<TPayload> dawgBuilder)
        {
            var dawg = dawgBuilder.BuildDawg ();

            var memoryStream = new MemoryStream ();

#pragma warning disable 612,618
            dawg.SaveAsYaleDawg (memoryStream);
#pragma warning restore 612,618

            var buffer = memoryStream.GetBuffer ();

            var rehydrated = Dawg<TPayload>.Load (new MemoryStream (buffer));

            return rehydrated;
        }
    }
}
