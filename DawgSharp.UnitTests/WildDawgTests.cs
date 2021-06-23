using System.Linq;
using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class WildDawgTests
    {
        [Test]
        public void Test1()
        {
            var builder = new DawgBuilder<int>();
            
            builder.Insert("газообразный".Reverse(), 5);
            builder.Insert("ракообразный".Reverse(), 5);
            builder.Insert("солнцеобразный".Reverse(), 5);
            builder.Insert("яйцеобразный".Reverse(), 5);
            builder.Insert("чашеобразный".Reverse(), 5);
            
            builder.Insert("образный".Reverse(), 8);

            WildDawg<int> dawg = builder.BuildWildDawg();

            Assert.AreEqual(8, dawg["образный".Reverse()]);
            
            Assert.AreEqual(5, dawg["газообразный".Reverse()]);
            Assert.AreEqual(5, dawg["ракообразный".Reverse()]);
            Assert.AreEqual(5, dawg["солнцеобразный".Reverse()]);
            Assert.AreEqual(5, dawg["яйцеобразный".Reverse()]);
            Assert.AreEqual(5, dawg["чашеобразный".Reverse()]);
            
            // These words have not been added and
            // there is not enough information to guess their payloads.
            Assert.AreEqual(0, dawg["разный".Reverse()]);
            Assert.AreEqual(0, dawg["заразный".Reverse()]);
            
            // These words have not been added but 
            // their payload should be inferred using wildcards.
            Assert.AreEqual(5, dawg["спицеобразный".Reverse()]);
            Assert.AreEqual(5, dawg["иглообразный".Reverse()]);
        }
    }
}