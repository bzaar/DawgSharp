using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class DawgBuilderTests
    {
        [TestMethod]
        public void EmptyKey ()
        {
            var db = new DawgBuilder<int>();

            int n;
            Assert.AreEqual(true, db.TryGetValue("", out n));
            Assert.AreEqual(0, n);
        }

        [TestMethod]
        public void IncrementValue ()
        {
            var db = new DawgBuilder<int>();

            Increment(db, "test");
            Increment(db, "test");
            Increment(db, "test");

            int n;
            Assert.AreEqual(true, db.TryGetValue("test", out n));
            Assert.AreEqual(3, n);
        }

        private static void Increment(DawgBuilder<int> db, string key)
        {
            int n;
            db.TryGetValue(key, out n);
            db.Insert(key, n + 1);
        }

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
