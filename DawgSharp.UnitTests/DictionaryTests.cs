using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class DictionaryTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("cone", 10);

            var dawg = dawgBuilder.BuildDawg ();

            Assert.AreEqual (10, dawg ["cone"]);
            Assert.AreEqual (0, dawg ["con"]);
            Assert.AreEqual (0, dawg ["cones"]);
            Assert.AreEqual (0, dawg ["pit"]);
        }

        [TestMethod]
        public void AssertNodeCount ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("taps", 10);
            dawgBuilder.Insert ("tops", 10);

            var dawg = dawgBuilder.BuildDawg ();

            Assert.AreEqual (6, dawg.GetNodeCount ());
        }

        [TestMethod]
        public void AssertNodeCount2 ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("probability", 10);
            dawgBuilder.Insert (  "stability", 10);

            var dawg = dawgBuilder.BuildDawg ();

            Assert.AreEqual (14, dawg.GetNodeCount ());
        }

        [TestMethod]
        public void AgoEgo ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("ago", 9);
            dawgBuilder.Insert ("ego", 10);

            var dawg = dawgBuilder.BuildDawg ();

            Assert.AreEqual (9,  dawg ["ago"]);
            Assert.AreEqual (10, dawg ["ego"]);
            Assert.AreEqual (0, dawg ["eg"]);
        }
    }
}
