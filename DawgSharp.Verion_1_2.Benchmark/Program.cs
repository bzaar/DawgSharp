extern alias Ver11;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DawgSharp.Verion_1_2.Benchmark
{
    class Program
    {
        private static readonly KeyValuePair<string, ushort> [] _wordNumberPairs = 
            File.ReadLines("two.million.words.txt", 
            Encoding.GetEncoding(1251)) // using a single byte encoding halves the size of the file
            .Select(GetKeyValuePair)
            .ToArray();

        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var tests = new Dictionary<string, Dictionary<string, double>>
            {
                {"Dictionary", RunTest(new DictionaryDawgFactory())},
                {"v. 1.1.1", RunTest(new OldDawgFactory())},
#pragma warning disable 612,618
                {"MatrixDawg", RunTest(new NewDawgFactory((d, s) => d.SaveAsMatrixDawg(s)))},
                {"v 1.2", RunTest(new NewDawgFactory((d, s) => d.SaveAsYaleDawg(s)))},
#pragma warning restore 612,618
            };

            Console.Write ("{0,22}:", "Name");

            foreach (var test in tests)
            {
                Console.Write (" {0,12}", test.Key);
            }

            Console.WriteLine();

            foreach (var metric1 in tests.First().Value)
            {
                Console.Write ("{0,22}:", metric1.Key);

                foreach (var test in tests)
                {
                    Console.Write (" {0,12:N0}", test.Value [metric1.Key]);
                }

                Console.WriteLine();
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == "DawgSharp, Version=1.1.1.0, Culture=neutral, PublicKeyToken=null")
            {
                return Assembly.LoadFrom(@"..\..\..\packages\DawgSharp.1.1.1\lib\net\DawgSharp.dll");
            }

            return null;
        }

        private static Dictionary<string, double> RunTest(IDawgFactory factory)
        {
            IEnumerable<KeyValuePair<string, ushort>> pairs = GetWordNumberPairs();

            var metrics = new Dictionary<string, double> ();

            IDawgBuilder dawgBuilder = null;

            var buildTime = Time(() =>
            {
                dawgBuilder = factory.CreateBuilder(pairs);
            });

            metrics.Add("Build Time, ms", buildTime);

            MemoryStream stream = null;

            var saveTime = Time(() =>
            {
                stream = new MemoryStream();
                dawgBuilder.Save(new NonDisposableStream(stream));
            });

            metrics.Add("Save Time, ms", saveTime);
            metrics.Add("File Size, bytes", stream.Position);
            stream.Position = 0;

            IDawg dawg = null;

            var before = GC.GetTotalMemory(true);

            var loadTime = Time(() =>
            {
                dawg = factory.Load(stream);
            });

            var after = GC.GetTotalMemory(true);

            metrics.Add("Load Time, ms", loadTime);
            metrics.Add("RAM, bytes", after - before);

            const int nLookups = 1000000;

            var lookupTime = Time(() =>
            {
                foreach (string word in RandomWords().Take(nLookups))
                {
                    if (dawg [word] == 0)
                    {
                        throw new Exception("dawg [" + word + "] == 0");
                    }
                }
            });

            metrics.Add("Lookups Per Sec", nLookups * 1000.0 / lookupTime);

            var matchPrefixTime = Time(() =>
            {
#pragma warning disable 219
                int x = 0;
#pragma warning restore 219

                foreach (var keyValuePair in RandomWords().SelectMany(w => dawg.MatchPrefix(w.Take(w.Length/2))).Take(nLookups))
                {
                    x += keyValuePair.Value;
                }
            });
            
            metrics.Add("Prefix Matches Per Sec", nLookups * 1000.0 / matchPrefixTime);

            return metrics;
        }

        static IEnumerable<string> RandomWords ()
        {
            string [] words = GetWordNumberPairs().Select(p => p.Key).ToArray();

            var rnd = new Random();

            for (;;)
            {
                yield return words [rnd.Next(words.Length)];
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns

        static long Time (Action test)
        {
            var stopwatch = Stopwatch.StartNew();
            test();
            return stopwatch.ElapsedMilliseconds;
        }

        private static IEnumerable<KeyValuePair<string, ushort>> GetWordNumberPairs()
        {
            return _wordNumberPairs;
        }

        private static KeyValuePair<string, ushort> GetKeyValuePair(string w)
        {
            int i = w.IndexOf('/');

            if (i == -1) i = w.IndexOf('ё'); else w = w.Remove(i, 1);

            return new KeyValuePair<string, ushort> (w, (ushort) (w.Length + 1 - i));
        }
    }

    interface IDawgFactory
    {
        IDawgBuilder CreateBuilder (IEnumerable<KeyValuePair<string, ushort>> pairs);
        IDawg Load (Stream stream);
    }

    interface IDawgBuilder
    {
        void Save (Stream stream);
    }

    interface IDawg
    {
        ushort this [IEnumerable<char> word] { get; }
        IEnumerable<KeyValuePair<string, ushort>> MatchPrefix (IEnumerable<char> word);
    }

    class NewDawg : IDawg
    {
        private readonly Dawg<ushort> dawg;

        public NewDawg (Dawg<ushort> dawg)
        {
            this.dawg = dawg;
        }

        ushort IDawg.this [IEnumerable<char> word]
        {
            get { return dawg [word]; }
        }

        public IEnumerable<KeyValuePair<string, ushort>> MatchPrefix(IEnumerable<char> word)
        {
            return dawg.MatchPrefix(word);
        }
    }

    class OldDawg : IDawg
    {
        private readonly Ver11::DawgSharp.Dawg<ushort> dawg;

        public OldDawg (Ver11::DawgSharp.Dawg<ushort> dawg)
        {
            this.dawg = dawg;
        }

        ushort IDawg.this [IEnumerable<char> word]
        {
            get { return dawg [word]; }
        }

        public IEnumerable<KeyValuePair<string, ushort>> MatchPrefix(IEnumerable<char> word)
        {
            return dawg.MatchPrefix(word);
        }
    }

    class DictionaryDawgFactory : IDawgFactory, IDawgBuilder, IDawg
    {
        private Dictionary<string, ushort> dict = new Dictionary<string, ushort>();

        public IDawgBuilder CreateBuilder(IEnumerable<KeyValuePair<string, ushort>> pairs)
        {
            foreach (var pair in pairs)
            {
                if (dict.ContainsKey(pair.Key)) continue;
                dict.Add(pair.Key, pair.Value);
            }

            return this;
        }

        public IDawg Load(Stream stream)
        {
            this.dict = (Dictionary<string, ushort>) new BinaryFormatter().Deserialize(stream);

            return this;
        }

        public void Save(Stream stream)
        {
            new BinaryFormatter ().Serialize(stream, dict);
        }

        ushort IDawg.this[IEnumerable<char> word]
        {
            get { return dict [word as string ?? new string(word.ToArray())]; }
        }

        IEnumerable<KeyValuePair<string, ushort>> IDawg.MatchPrefix(IEnumerable<char> word)
        {
            return Enumerable.Repeat (new KeyValuePair<string, ushort>("asdf", 1), 3);
        }
    }

    class NewDawgFactory : IDawgFactory
    {
        private readonly Action<Dawg<ushort>, Stream> save;

        public NewDawgFactory (Action<Dawg<ushort>, Stream> save)
        {
            this.save = save;
        }

        IDawgBuilder IDawgFactory.CreateBuilder(IEnumerable<KeyValuePair<string, ushort>> pairs)
        {
            return new NewDawgBuilder(DawgExtensions.ToDawg(pairs, p => p.Key, p => p.Value), save);
        }

        IDawg IDawgFactory.Load(Stream stream)
        {
            return new NewDawg (Dawg<ushort>.Load(stream));
        }
    }

    class OldDawgFactory : IDawgFactory
    {
        IDawgBuilder IDawgFactory.CreateBuilder(IEnumerable<KeyValuePair<string, ushort>> pairs)
        {
            return new OldDawgBuilder (Ver11::DawgSharp.DawgExtensions.ToDawg(pairs, p => p.Key, p => p.Value));
        }

        IDawg IDawgFactory.Load(Stream stream)
        {
            return new NewDawg (Dawg<ushort>.Load(stream, r => r.ReadUInt16 ()));
        }
    }
}
