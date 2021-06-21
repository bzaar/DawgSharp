using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    static class LevelBuilder <TPayload>
    {
        public static void MergeEnds (Node <TPayload> root)
        {
            var levels = new[] {NewLevel()}.ToList();

            var stack = new Stack <StackFrame> ();

            Push (stack, root);

            while (stack.Count > 0)
            {
                if (stack.Peek().ChildIterator.MoveNext ())
                {
                    // go deeper
                    Push (stack, stack.Peek().ChildIterator.Current.Value);
                }
                else
                {
                    var current = stack.Pop ();

                    if (stack.Count > 0)
                    {
                        var parent = stack.Peek ();

                        int level = current.Level;

                        if (levels.Count <= level)
                        {
                            levels.Add (NewLevel());
                        }

                        var dictionary = levels [level];

                        var nodeWrapper = new NodeWrapper <TPayload> (
                            current.Node,
                            parent.Node,
                            parent.ChildIterator.Current.Key);

                        if (dictionary.TryGetValue (nodeWrapper, out NodeWrapper<TPayload> existing))
                        {
                            parent.Node.Children [parent.ChildIterator.Current.Key] = existing.Node;
                        }
                        else
                        {
                            dictionary.Add (nodeWrapper, nodeWrapper);
                        }

                        int parentLevel = current.Level + 1;

                        if (parent.Level < parentLevel)
                        {
                            parent.Level = parentLevel;
                        }
                    }
                }
            }
        }

        private static Dictionary<NodeWrapper<TPayload>, NodeWrapper<TPayload>> NewLevel() => new(Comparer);
        private static readonly NodeWrapperEqualityComparer<TPayload> Comparer = new ();

        private static void Push (Stack <StackFrame> stack, Node <TPayload> node)
        {
            stack.Push (new StackFrame {Node = node, ChildIterator = node.Children.ToList ().GetEnumerator ()});
        }

        class StackFrame
        {
            public Node <TPayload> Node;
            public IEnumerator <KeyValuePair <char, Node <TPayload>>> ChildIterator;
            public int Level;
        }
    }
}
