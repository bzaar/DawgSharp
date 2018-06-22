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
            return DawgBuilder<TPayload>.BuildYaleDawg(dawgBuilder);
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
