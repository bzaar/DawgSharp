using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    /// <summary>
    /// Summary description for MatchPrefixTests
    /// </summary>
    [TestClass]
    public class MatchPrefixTests
    {
        [TestMethod]
        public void TestMethod1 ()
        {
            var dawgBuilder = new DawgBuilder<bool> ();

            dawgBuilder.Insert ("cat", true);
            dawgBuilder.Insert ("caterpillar", true);
            dawgBuilder.Insert ("dog", true);

            var dawg = dawgBuilder.BuildDawg ();

            Assert.IsTrue (dawg.MatchPrefix ("cat").Select (kvp => kvp.Key).SequenceEqual (new [] {"cat", "caterpillar"}));
            Assert.IsTrue (dawg.MatchPrefix ("ca").Select (kvp => kvp.Key).SequenceEqual (new [] {"cat", "caterpillar"}));
            Assert.IsTrue (dawg.MatchPrefix ("").Select (kvp => kvp.Key).SequenceEqual (new [] {"cat", "caterpillar", "dog"}));
            Assert.IsTrue (dawg.MatchPrefix ("boot").Count () == 0);
            Assert.IsTrue (dawg.MatchPrefix ("cats").Count () == 0);
        }

        [TestMethod]
        public void EmptyDictioinaryTest ()
        {
            var dawgBuilder = new DawgBuilder<bool> ();

            var dawg = dawgBuilder.BuildDawg ();

            Assert.IsTrue (dawg.MatchPrefix ("boot").Count () == 0);
            Assert.IsTrue (dawg.MatchPrefix ("").Count () == 0);
        }
    }
}
