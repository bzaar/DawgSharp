[NuGet Package](https://www.nuget.org/packages/DawgSharp/)

DawgSharp, a clever string dictionary in C#
===========================================

DAWG (Directed Acyclic Word Graph) is a data structure for storing and searching large word lists and dictionaries.  It can be 40x more efficient than the .NET ```Dictionary``` class for certain types of data.

As an example, [my website](http://russiangram.com) hosts a 2 million word dictionary which used to take up 56 meg on disk and took 7 seconds to load.  After switching to DAWG, it now takes 1.4 meg on disk and  0.3 seconds to load.

How is this possible?  Why is the standard Dictionary not as clever as DAWG?  The thing is, DAWG works well with natural language strings and may not work as well for generated strings such as license keys (OIN1r4Be2su+UXSeOj0TaQ).  Human language words tend to have lots of common letter sequences eg _-ility_ in _ability_, _possibility_, _agility_ etc and the algorithm takes advantage of that by finding those sequences and storing them only once for all words.  DAWG has also proved useful in representing DNA data (sequences of genes).  The history of DAWG dates back as far as 1985.  For more backgroud, google DAWG or DAFSA (Deterministic Acyclic Finite State Automaton).

DawgSharp is an implementation of DAWG, one of many.  What makes it special?

 * It is written in pure C#, compiles to MSIL (AnyCPU) and works on .NET 3.5 and above.
 * It has no dependencies.
 * It introduces no limitations on characters in keys.  Some competing implementations allow only 26 English letters.  This implementation handles any Unicode characters.
 * The compaction algorithm visits every node only once which makes it really fast (10 seconds for my 2 million word list).
 * It offers out-of-the-box persistence: call ```Load/Save``` to write the data to disk and read it back.
 * It has unit tests (using the Visual Studio testing framework).

Usage
-----
In this example we will simulate a usage scenario involving two programs, one to generate the dictionary and write it to disk and the other to load that file and use the read-only dictionary for lookups.

First get the code by cloning this repository or install the [NuGet package](https://www.nuget.org/packages/DawgSharp/).

Create and populate a ```DawgBuilder``` object:

```
var dawgBuilder = new DawgBuilder <bool> (); // more on <bool> below

foreach (string key in ...)
{
  dawgBuilder.Insert (key, true);
}
```

Call ```BuildDawg``` on it to get the compressed version and save it to disk:

```
var dawg = dawgBuilder.BuildDawg (); // Computer is working.  Please wait ...

dawg.Save (File.Create ("DAWG.bin"), 
  (writer, value) => writer.Write (value)); // explained below
```

Now read the file back in and check if a particular word is in the dictionary:

```
var dawg = Dawg <bool>.Load (File.Open ("DAWG.bin"), 
   reader => reader.ReadBool ());           // explained below

if (dawg ["chihuahua"])
{
  Console.WriteLine ("Word is found.");
}
```

&lt;TPayload&gt;
----------

The ```Dawg``` and ```DawgBuilder``` classes take a template parameter called ```<TPayload>```.  It can be any type you want.  Just to be able to test if a word is in the dictionary, a bool is enough.  You can also make it an ```int``` or a ```string``` or a custom class.  But beware of one important limitation.  DAWG works well only when the set of values that TPayload can take is relatively small.  The smaller the better.  Eg if you add a definition for each word, it will make each entry unique and it won't be able to compact the graph and thus you will loose all the benefits of DAWG.

Now, about those lambdas that you pass to ```Load``` and ```Save```.  This is how you tell these methods how to serialize and deserialize TPayload.  Since you choose the type, you must tell the library how to serialize it.  You must write something to the BinaryWriter, even if the value of TPayload is ```null```.

Thread Safety
-------------

The ```DawgBuilder``` class is *not* thread-safe and must be accessed by only one thread at any particular time.

The ```Dawg``` class is immutable and thus thread-safe.

Future plans
------------
###Better API

The API was designed to fit a particular usage scenario (see above) and can be extended to support other scenarios eg being able to add new words to the dictionary after it's been compacted.  I just didn't need this so it's not implemented.  You won't get any exceptions.  There is just no ```Insert``` method on the ```Dawg``` class.

###More optimizations

I went from not knowing anything about DAWGs to a complete implementation and a real life application in just two days.  While writing the code, I always went for the simplest thing that could work and haven't done much optimization since.  I was happy with the 20x speed increase and a 40x file size reduction that I got by switching to DAWG.  That said, there is still a lot of potential for optimization both for speed and disk/RAM usage.  If you feel like going for it, just fork this repo and optimize away.  There are unit tests in place to make sure you don't break anything.
