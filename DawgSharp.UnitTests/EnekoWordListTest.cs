using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class EnekoWordListTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var words = File.ReadAllLines (@"..\..\..\..\..\eneko-words.txt");

            var builder = new DawgBuilder<bool> ();

            foreach (var word in words)
            {
                builder.Insert (word, true);
            }

            builder.BuildDawg ();
        }
    }
}
