using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class MultiDawgTests
    {
        [Test]
        [TestCase("ё")]
        [TestCase("сё")]
        [TestCase("сёл")]
        [TestCase("ёж")]
        public void MatchTreeTest(string yo)
        {
            var builder = new MultiDawgBuilder<int>();
            string ye = yo.Replace('ё', 'е');
            builder.Insert(ye, new [] {1});
            builder.Insert(yo, new [] {2});
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            AssertSequenceEquals(multiDawg.MatchTree(ye.Select(YeToYeYo)).SelectMany(p => p.Value), 1, 2);
            AssertSequenceEquals(multiDawg.MatchTree(yo.Select(YeToYeYo)).SelectMany(p => p.Value), 2);
        }

        static IEnumerable<char> YeToYeYo(char c)
        {
            yield return c;

            if (c == 'е')
            {
                yield return 'ё';
            }
        }

        [Test]
        public void Test1()
        {
            var builder = new MultiDawgBuilder<int>();
            builder.Insert("apple", new [] {1, 3, 2});
            builder.Insert("orange", new [] {5, 3, 7});
            builder.Insert("empty", new int [0]);
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            AssertSequenceEquals(multiDawg["apple"], 1, 3, 2);
            AssertSequenceEquals(multiDawg["orange"], 5, 3, 7);
            AssertSequenceEquals(multiDawg["empty"]);
            AssertSequenceEquals(multiDawg["not found"]);
        }

        [Test]
        [TestCase(new []{"big"}, 1, 1)]
        [TestCase(new []{"big", "apple"}, 2, 2)]
        [TestCase(new []{"big", "app"}, 1, 1)] // only finds "big", ignoring "app"
        public void MultiwordFindTestBigApple(string[] key, int wordsFound, params int[] values)
        {
            var builder = new MultiDawgBuilder<int>();
            builder.Insert("big", new [] {1});
            builder.Insert("big apple", new [] {2});
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            
            AssertSequenceEquals(multiDawg.MultiwordFind(key, out int wordCount), values);
            Assert.AreEqual(wordCount, wordsFound);
        }

        [Test]
        [TestCase(new []{"a", "b"}, 2, 1, 3, 2)]
        [TestCase(new []{"a"}, 1, 5, 3, 7)]
        [TestCase(new []{"not found"}, 0)]
        [TestCase(new []{"x yy zzz"}, 1, 1)]
        [TestCase(new []{"x yy"}, 0)]
        [TestCase(new []{"x yy zz"}, 0)]
        [TestCase(new []{"x"}, 1, 2)]
        public void MultiwordFindTestRandomValues(string[] key, int wordsFound, params int[] values)
        {
            var builder = new MultiDawgBuilder<int>();
            builder.Insert("a b", new [] {1, 3, 2});
            builder.Insert("a", new [] {5, 3, 7});
            builder.Insert("x yy zzz", new [] {1});
            builder.Insert("x", new [] {2});
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            
            AssertSequenceEquals(multiDawg.MultiwordFind(key, out int wordCount), values);
            Assert.AreEqual(wordCount, wordsFound);
        }

        [Test]
        public void MergeEndsTest()
        {
            var builder = new MultiDawgBuilder<int>();
            builder.Insert("ax", new [] {1, 2, 3});
            builder.Insert("bx", new [] {1, 2, 3});
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            
            // The three nodes should be:
            // 1. root with two children, "a" and "b", pointing to the *same* node "2"
            // 2. node "2" pointing to node "x".
            // 3. node "x" carrying the payload 1,2,3.
            Assert.AreEqual(3, multiDawg.GetNodeCount());
        }

        [Test]
        public void EmptyMultiDawgTest()
        {
            var builder = new MultiDawgBuilder<int>();
            MultiDawg<int> multiDawg = builder.BuildMultiDawg();
            
            Assert.AreEqual(1, multiDawg.GetNodeCount()); // the root node
            AssertSequenceEquals(multiDawg.MultiwordFind(new []{""}, out int wordCount), new int[0]);
            Assert.AreEqual(0, wordCount);
        }

        private static void AssertSequenceEquals(IEnumerable<int> values, params int[] expected)
        {
            Assert.AreEqual(ToString(expected), ToString(values));
        }

        static string ToString(IEnumerable<int> seq) => string.Join(" ", seq);
    }
}