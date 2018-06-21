using System.IO;
using System.Linq;
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

        [Test]
        public void GetPrefixesTest()
        {
            var dawgBuilder = new DawgBuilder<bool>();

            dawgBuilder.Insert("read", true);
            dawgBuilder.Insert("reading", true);

            var rehydrated = GetDawg(dawgBuilder);

            Assert.AreEqual("read,reading", string.Join(",", rehydrated.GetPrefixes("readings").Select(kvp => kvp.Key)));
        }

        [Test]
        public void GetPrefixesOnEmptyGraph()
        {
            var dawgBuilder = new DawgBuilder<bool>();

            var rehydrated = GetDawg(dawgBuilder);

            Assert.AreEqual(0, rehydrated.GetPrefixes("readings").Count());
        }

        [Test]
        public void GetPrefixesOnEmptyString()
        {
            var dawgBuilder = new DawgBuilder<bool>();

            dawgBuilder.Insert("", true);

            var rehydrated = GetDawg(dawgBuilder);

            Assert.AreEqual("", rehydrated.GetPrefixes("readings").Single().Key);
        }
    }
}
