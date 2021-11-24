using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class OldDawgTests : DawgTests
    {
        protected override Dawg<TPayload> GetDawg <TPayload> (DawgBuilder<TPayload> dawgBuilder)
        {
            return dawgBuilder.BuildDawg ();
        }
    }
}