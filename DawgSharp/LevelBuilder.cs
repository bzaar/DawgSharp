using System.Collections.Generic;

namespace DawgSharp
{
    class LevelBuilder <TPayload>
    {
        public static List <List <NodeWrapper <TPayload>>> BuildLevels (Node <TPayload> root)
        {
            var levels = new List <List <NodeWrapper <TPayload>>> ();

            var stack = new Stack <StackNode <TPayload>> ();

            Push (stack, root);

            while (stack.Count > 0)
            {
                if (stack.Peek ().ChildIterator.MoveNext ())
                {
                    // go deeper
                    Push (stack, stack.Peek ().ChildIterator.Current.Value);
                }
                else
                {
                    var current = stack.Pop ();

                    if (stack.Count > 0)
                    {
                        int level = current.Level;

                        if (levels.Count <= level)
                        {
                            levels.Add (new List <NodeWrapper <TPayload>> ());
                        }

                        var list = levels [level];

                        list.Add (new NodeWrapper <TPayload> (current.Node, stack.Peek ().Node, stack.Peek ().ChildIterator.Current.Key));

                        int superLevel = current.Level + 1;

                        if (stack.Peek ().Level < superLevel)
                        {
                            stack.Peek ().Level = superLevel;
                        }
                    }
                }
            }

            return levels;
        }

        private static void Push (Stack <StackNode <TPayload>> stack, Node <TPayload> node)
        {
            stack.Push (new StackNode <TPayload> {Node = node, ChildIterator = node.Children.GetEnumerator ()});
        }
    }

    class StackNode <TPayload>
    {
        public Node <TPayload> Node;
        public IEnumerator <KeyValuePair <char, Node <TPayload>>> ChildIterator;
        public int Level;
    }
}