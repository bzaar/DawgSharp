[![Build status](https://ci.appveyor.com/api/projects/status/4htqh2lt5l5vfgxd?svg=true)](https://ci.appveyor.com/project/morpher/dawgsharp)
  [[NuGet Package](https://www.nuget.org/packages/DawgSharp/)]   [[Get Commercial License](http://morpher.co.uk)]   

DawgSharp, a clever string dictionary in C#
===========================================

DAWG (Directed Acyclic Word Graph) is a data structure for storing and searching large word lists and dictionaries.  It can be 40x more efficient than the .NET ```Dictionary``` class for certain types of data.

As an example, [my website](http://russiangram.com) hosts a 2 million word dictionary which used to take up 56 meg on disk and took 7 seconds to load (when using Dictionary and BinarySerializer).  After switching to DAWG, it now takes 1.4 meg on disk and 0.3 seconds to load.

How is this possible?  Why is the standard Dictionary not as clever as DAWG?  The thing is, DAWG works well with natural language strings and may not work as well for generated strings such as license keys (OIN1r4Be2su+UXSeOj0TaQ).  Human language words tend to have lots of common letter sequences eg _-ility_ in _ability_, _possibility_, _agility_ etc and the algorithm takes advantage of that by finding those sequences and storing them only once for multiple words.  DAWG has also proved useful in representing DNA data (sequences of genes).  The history of DAWG dates back as far as 1985.  For more backgroud, google DAWG or DAFSA (Deterministic Acyclic Finite State Automaton).

DawgSharp is an implementation of DAWG, one of many.  What makes it special?

 * It is written in pure C#, compiles to MSIL (AnyCPU) and runs on .NET 3.5 and above.
 * It has no dependencies.
 * It introduces no limitations on characters in keys.  Some competing implementations allow only 26 English letters.  This implementation handles any Unicode characters.
 * The compaction algorithm visits every node only once which makes it really fast (5 seconds for my 2 million word list).
 * It offers out-of-the-box persistence: call ```Load/Save``` to write the data to disk and read it back.
 * It has unit tests (using the Visual Studio testing framework).
 * It has received several man-hours of performance profiling sessions so it's pretty much as fast as it can be. The next step of making it faster would be rewriting the relevant code in IL.
 * It's GC-friendly: the Garbage Collector only sees 3 large arrays of ints where a Dictionary would store millions of strings.

Usage
-----
In this example we will simulate a usage scenario involving two programs, one to generate the dictionary and write it to disk and the other to load that file and use the read-only dictionary for lookups.

First get the code by cloning this repository or installing the [NuGet package](https://www.nuget.org/packages/DawgSharp/).

Create and populate a ```DawgBuilder``` object:

```csharp
var dawgBuilder = new DawgBuilder <bool> (); // <bool> is the value type.
                                             // Key type is always string.

foreach (string key in new [] {"Aaron", "abacus", "abashed"})
{
    dawgBuilder.Insert (key, true);
}
```

Call ```BuildDawg``` on it to get the compressed version and save it to disk:

```csharp
var dawg = dawgBuilder.BuildDawg (); // Computer is working.  Please wait ...

using (var file = File.Create ("DAWG.bin")) 
    dawg.SaveTo (file);
```

Now read the file back in and check if a particular word is in the dictionary:

```csharp
var dawg = Dawg <bool>.Load (File.Open ("DAWG.bin"));

if (dawg ["chihuahua"])
{
    Console.WriteLine ("Word is found.");
}
```

The Value Type, &lt;TPayload&gt;
----------

The ```Dawg``` and ```DawgBuilder``` classes take a template parameter called ```<TPayload>```.  It can be any type you want.  Just to be able to test if a word is in the dictionary, a bool is enough.  You can also make it an ```int``` or a ```string``` or a custom class.  But beware of one important limitation.  DAWG works well only when the set of values that TPayload can take is relatively small.  The smaller the better.  Eg if you add a definition for each word, it will make each entry unique and it won't be able to compact the graph and thus you will loose all the benefits of DAWG.

MatchPrefix()
-------------
One other attractive side of DAWG is its ability to efficiently retrieve all words starting with a particular substring:

```csharp
dawg.MatchPrefix("awe")
```

The above query will return an ```IEnumerable<KeyValuePair>``` which might contain keys such as **awe, aweful** and **awesome**. The call ```dawg.MatchPrefix("")``` will return all items in the dictionary.

If you need to look up by suffix instead, there is no MatchSuffix method. But the desired effect can be achieved
by adding the reversed keys and then using MatchPrefix() on the reversed keys:

```csharp
dawgBuilder.Insert("ability".Reverse(), true);
...
dawg.MatchPrefix("ility".Reverse())
```

GetPrefixes()
-------------

GetPrefixes() returns all dictionary items whose keys are substrings of a given string. For example:

```csharp
dawg.GetPrefixes("awesomenesses")
```
Might return keys such as **awe, awesome, awesomeness** and finally **awesomenesses**.

GetLongestCommonPrefixLength()
------------------------------

One other neat feature is the method ```int GetLongestCommonPrefixLength(IEnumerable<char> word)```. If ```word``` is found in the dictionary, it will return its length; if not, it will return the length of the longest word that *is* found in the dictionary and that is also the beginning of the given word. For example, if **prepare** is in the dictionary but **preempt** is not, then ```dawg.GetLongestCommonPrefixLength("preempt")``` will return 3 which is the length of "pre".

Thread Safety
-------------

The ```DawgBuilder``` class is *not* thread-safe and must be accessed by only one thread at any particular time.

The ```Dawg``` class is immutable and thus thread-safe.

Future plans
------------
### More usage scenarios

The API was designed to fit a particular usage scenario (see above) and can be extended to support other scenarios eg being able to add new words to the dictionary after it's been compacted.  I just didn't need this so it's not implemented.  You won't get any exceptions.  There is just no ```Insert``` method on the ```Dawg``` class.

### Better API

Implement the IDictionary interface on both DawgBuilder and Dawg ([#5](https://github.com/bzaar/DawgSharp/issues/5)).

Literature
----------
 * [Comparisons of Efficient Implementations for DAWG](http://www.ijcte.org/vol8/1018-C024.pdf)
 * [DotNetPerls](http://www.dotnetperls.com/directed-acyclic-word-graph)
 * [Radix trie - Wikipedia](https://en.wikipedia.org/wiki/Radix_tree)
 * [http://wutka.com/dawg.html](http://wutka.com/dawg.html)

Competing Implementations
-------------------------
 * [DAWG (C#)](https://www.nuget.org/packages/DAWG)
 * [dawgdic (C++)](https://code.google.com/p/dawgdic/)
 * [MARISA (C++)](https://code.google.com/p/marisa-trie/)
 * [libdatrie (C)](http://linux.thai.net/~thep/datrie/datrie.html)

License
-------
DawgSharp is licensed under GPLv3 which means it can be used free of charge in open-sources projects. [Read the full license](LICENSE.txt)

If you would like to use DawgSharp in a proprietary project, please purchase a commercial license at [http://morpher.co.uk](http://morpher.co.uk).
