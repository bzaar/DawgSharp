using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DawgSharp.UnitTests
{
    [TestClass]
    public class PersistenceTests
    {
        [TestMethod]
        public void PersistenceTest ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("cone", 10);
            dawgBuilder.Insert ("bone", 10);
            dawgBuilder.Insert ("gone", 9);
            dawgBuilder.Insert ("go", 5);

            var dawg = dawgBuilder.BuildDawg ();

            var memoryStream = new MemoryStream ();

            dawg.SaveTo (memoryStream, (w, p) => w.Write (p));

            var buffer = memoryStream.GetBuffer ();

            var rehydrated = Dawg<int>.Load (new MemoryStream (buffer), r => r.ReadInt32 ());

            Assert.AreEqual (10, rehydrated ["cone"]);
            Assert.AreEqual (10, rehydrated ["bone"]);
            Assert.AreEqual (0, rehydrated ["cones"]);
            Assert.AreEqual (9, rehydrated ["gone"]);
            Assert.AreEqual (5, rehydrated ["go"]);
            Assert.AreEqual (0, rehydrated ["god"]);
        }
    }
}
