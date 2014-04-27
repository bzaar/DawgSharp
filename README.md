DawgSharp, a clever string dictionary in C#
===============================================

DAWG stands for 'Directed Acyclic Word Graph' and is a data structure for effectively storing and searching large word lists.  It is basically a Dictionary &lt;string, T&gt;, only a lot faster.  Just for your reference, replacing the standard Dictionary with DawgSharp in my website that uses a dictionary of over 2 million words has cut down the load time from 7 seconds to 0.3 seconds and the file size from 56M to 1.4M.

How is this possible?  Why is the standard Dictionary not as clever as the DAWG?  The thing is, the DAWG works well with natural language strings and may not work as well for generated strings such as license keys (OIN1r4Be2su+UXSeOj0TaQ).  Human language words tend to have lots of common letter sequences eg _-ility_ in _ability_, _possibility_, _agility_ etc and the algorithm takes advantage of that by finding those sequences and storing them only once for all words.  The history of DAWG dates back as far as 1985.  For more backgroud google DAWG or DAFSA (Deterministic Acyclic Finite State Automaton).

The above is true for the DAWG data structure in general.  Now more about what is specific to DawgSharp.

This implementation features:
 * Pure C# code that compiles to MSIL (AnyCPU) and works on .NET 3.5 and above.
 * No dependencies.
 * No limitations on characters in keys.  Some competing implementations allow only 26 English letters.  This implementation can handle any Unicode characters.
 * The compaction algorithm visits every node only once which makes it really fast (10 seconds for my 2 million word list) on a single CPU core.
 * Load/Save methods to write the data to disk and read it back.
 * Unit tests (using the Visual Studio testing framework).


Usage
-----
My usage scenario involved two programs, one to generate the dictionary and write it to disk and the other to load that file and use the read-only dictionary for lookups.

You start by creating and populating a DawgBuilder object:

```
var dawgBuilder = new DawgBuilder <bool> ();

foreach (string key in ...)
{
  dawgBuilder.Insert (key, true);
}
```

And then you call BuildDawg on it to get the compressed version and save it to disk:

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

The Dawg and DawgBuilder classes take a template parameter called ```<TPayload>```.  It can be any type you want.  Just to be able to test if a word is in the dictionary, a bool is enough.  You can also make it an int or a string or a custom class.  But beware of one important limitation.  The DAWG works well only when the set of values that TPayload can take is comparatively small.  The smaller the better.  Eg if you add a definition for each word, it will make each entry unique and it won't be able to compact the graph and thus you will loose all the benefits of DAWG.

Now, about those lambdas that you pass to ```Load``` and ```Save```.  This is how you tell these methods how to serialize and deserialize TPayload.  Since you choose the type, you must tell the library how to serialize it.  You must write something to the BinaryWriter, even if the value of TPayload is ```null```.

Future plans
------------
###Better API

The API was designed to fit a particular usage scenario (see above) and can be extended to support other scenarios eg being able to add new words to the dictionary after it's been compacted.  I just didn't need this so it's not implemented.  You won't get any exceptions.  There is just no ```Insert``` method on the ```Dawg``` class.

###More optimizations

I went from not knowing anything about DAWGs to a complete implementation and a real life application in just two days.  While writing the code, I always went for the simplest thing that could work and haven't done much optimization since.  I was happy with the 20x speed increase and a 40x file size reduction that I got by switching to DAWG.  That said, there is still a lot of potential for optimization both for speed and disk/RAM usage.  If you feel like going for it, just fork this repo and optimize away.  There are unit tests in place to make sure you don't break anything.
