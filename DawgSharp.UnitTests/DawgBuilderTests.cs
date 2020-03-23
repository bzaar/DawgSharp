using System.Linq;
using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class DawgBuilderTests
    {
        [Test]
        public void EmptyKey ()
        {
            var db = new DawgBuilder<int>();

            int n;
            Assert.False(db.TryGetValue("", out n));
            Assert.AreEqual(0, n);
        }

        [Test]
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

        [Test]
        public void TryGetValueOnPartialKey()
        {
            var builder = new DawgBuilder<bool>();
            builder.Insert("dates", true);
            bool b = builder.TryGetValue("date", out var v);
            Assert.False(v);
            Assert.False(b);
        }

        private static void Increment(DawgBuilder<int> db, string key)
        {
            int n;
            db.TryGetValue(key, out n);
            db.Insert(key, n + 1);
        }

        [Test]
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
