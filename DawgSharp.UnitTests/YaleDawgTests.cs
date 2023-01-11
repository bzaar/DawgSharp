using System.Linq;
using NUnit.Framework;

namespace DawgSharp
{
    [TestFixture]
    public class YaleDawgTests : DawgTests
    {
        protected override Dawg<TPayload> GetDawg <TPayload> (DawgBuilder<TPayload> dawgBuilder)
        {
            return dawgBuilder.BuildYaleDawg();
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
        public void GetPrefixesWithKeyShorterThanItem()
        {
            var dawgBuilder = new DawgBuilder<bool>();

            dawgBuilder.Insert("ab", true);

            var rehydrated = GetDawg(dawgBuilder);

            Assert.AreEqual("", string.Join(",", rehydrated.GetPrefixes("a").Select(kvp => kvp.Key)));
        }

        [Test]
        public void GetPrefixesWithKeySameLengthAsItem()
        {
            var dawgBuilder = new DawgBuilder<bool>();

            dawgBuilder.Insert("ab", true);

            var rehydrated = GetDawg(dawgBuilder);

            Assert.AreEqual("ab", string.Join(",", rehydrated.GetPrefixes("ab").Select(kvp => kvp.Key)));
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
