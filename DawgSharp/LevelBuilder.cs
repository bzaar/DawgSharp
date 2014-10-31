using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    class LevelBuilder <TPayload>
    {
        private readonly List <List <NodeWrapper <TPayload>>> levels = new List <List <NodeWrapper <TPayload>>> ();

        public List <List <NodeWrapper <TPayload>>> Levels
        {
            get { return levels; }
        }

        public int GetLevel (Node <TPayload> node, Node <TPayload> super, char @char)
        {
            var level = node.HasChildren () ? node.Children.Max (child => GetLevel (child.Value, node, child.Key)) + 1 : 0;

            if (super != null)
            {
                if (Levels.Count <= level)
                {
                    Levels.Add (new List <NodeWrapper <TPayload>> ());
                }

                var list = Levels [level];

                list.Add (new NodeWrapper <TPayload> (node, super, @char));
            }

            return level;
        }

        public static List <List <NodeWrapper <TPayload>>> BuildLevels (Node <TPayload> root)
        {
            var levelBuilder = new LevelBuilder<TPayload> ();

            levelBuilder.GetLevel (root, null, ' ');

            return levelBuilder.Levels;
        }
    }
}