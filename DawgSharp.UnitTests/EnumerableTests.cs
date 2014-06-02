using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class EnumerableTests
    {
        [TestMethod]
        public void IEnumerableCountWorksForDawg ()
        {
            var fruit = new [] {"apple", "banana", "orange"};

            var dawg = fruit.ToDawg (f => f, f => true);

            Assert.IsTrue (dawg ["apple"]);
            Assert.IsTrue (dawg ["banana"]);
            Assert.IsTrue (dawg ["orange"]);
            Assert.IsFalse (dawg ["kiwi"]);

            Assert.AreEqual (3, dawg.Count ());
        }
    }
}
