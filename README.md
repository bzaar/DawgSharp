DawgSharp
=========

DAWG String Dictionary - C# version

DAWG stands for 'Directed Acyclic Word Graph' and is a data structure for effectivly storing and searching large word lists.  It is basically a Dictionary <string, T>, only a lot faster.  Just for your reference, replacing the standard Dictionary with DawgSharp in my website that uses a dictionary of over 2 million words has cut down the load time from 7 seconds to 0.3 seconds and the file size from 56M to 1.4M.

How is this possible?  Why is the standard Dictionary not as clever as the DAWG?  The thing is, the DAWG works well with natural language strings and may not work as well for generated strings such as license keys ().  Human language words tend to have lots of common letter sequences eg -ility in ability, possibility, agility etc and the algorithm takes advantage of that by finding those sequences and storing them only once for all words.

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


