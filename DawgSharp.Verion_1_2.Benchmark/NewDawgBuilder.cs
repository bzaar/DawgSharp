using System;
using System.IO;

namespace DawgSharp.Verion_1_2.Benchmark
{
    class NewDawgBuilder : IDawgBuilder
    {
        private readonly Dawg<ushort> dawg;
        private readonly Action<Dawg<ushort>, Stream> save;

        public NewDawgBuilder (Dawg<ushort> dawg, Action<Dawg<ushort>, Stream> save)
        {
            this.dawg = dawg;
            this.save = save;
        }

        void IDawgBuilder.Save(Stream stream)
        {
            save (dawg, stream);
        }
    }
}