extern alias Ver11;

namespace DawgSharp.Verion_1_2.Benchmark
{
    class OldDawgBuilder : IDawgBuilder
    {
        private readonly Ver11::DawgSharp.Dawg<ushort> dawg;

        public OldDawgBuilder(Ver11::DawgSharp.Dawg<ushort> dawg)
        {
            this.dawg = dawg;
        }

        void IDawgBuilder.Save(System.IO.Stream stream)
        {
            dawg.SaveTo(stream, (w, p) => w.Write(p));
        }
    }
}
