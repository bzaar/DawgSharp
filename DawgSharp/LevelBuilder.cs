using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class LevelBuilder <TPayload>
    {
        public LevelBuilder(IEqualityComparer<TPayload> comparer = null)
        {
            this.comparer = new NodeWrapperEqualityComparer<TPayload>(comparer ?? EqualityComparer<TPayload>.Default);
        }
        
        public void MergeEnds (Node <TPayload> root)
        {
            var levels = new[] {NewLevel()}.ToList();

            var stack = new Stack <StackFrame> ();

            Push (stack, root);

            while (stack.Count > 0)
            {
                if (stack.Peek().ChildIterator.MoveNext ())
                {
                    // depth first
                    Push (stack, stack.Peek().ChildIterator.Current.Value);
                }
                else
                {
                    StackFrame current = stack.Pop ();

                    if (stack.Count > 0)
                    {
                        StackFrame parent = stack.Peek ();

                        if (levels.Count <= current.Level)
                        {
                            levels.Add (NewLevel());
                        }

                        var level = levels [current.Level];

                        var nodeWrapper = new NodeWrapper<TPayload>(current.Node);

                        if (level.TryGetValue (nodeWrapper, out NodeWrapper<TPayload> existing))
                        {
                            parent.Node.Children [parent.ChildIterator.Current.Key] = existing.Node;
                        }
                        else
                        {
                            level.Add (nodeWrapper, nodeWrapper);
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

        private Dictionary<NodeWrapper<TPayload>, NodeWrapper<TPayload>> NewLevel() => new(comparer);
        private readonly NodeWrapperEqualityComparer<TPayload> comparer;

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
