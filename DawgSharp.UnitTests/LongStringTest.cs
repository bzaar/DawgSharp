using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class LongStringTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string longString = new string ('a', 200 * 1000);

            var builder = new DawgBuilder<bool> ();

            builder.Insert (longString, true);

            var dawg = builder.BuildDawg ();

            Assert.IsTrue (dawg [longString]);
        }
    }
}
