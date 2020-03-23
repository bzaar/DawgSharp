using System;
using System.Collections.Generic;
using System.IO;

namespace DawgSharp
{
    public class DawgBuilder <TPayload>
    {
        readonly Node <TPayload> root = new Node <TPayload> ();

        readonly List <Node <TPayload>> lastPath = new List <Node <TPayload>> ();
        string lastKey = "";

        /// <summary>
        /// Inserts a new key/value pair or updates the value for an existing key.
        /// </summary>
        public void Insert (IEnumerable<char> key, TPayload value)
        {
            if (key is string strKey)
            {
                InsertLastPath(strKey, value);
            }
            else
            {
                DoInsert(root, key, value);
            }
        }

        private void InsertLastPath(string strKey, TPayload value)
        {
            int i = 0;

            while (i < strKey.Length && i < lastKey.Length)
            {
                if (strKey [i] != lastKey [i]) break;
                ++i;
            }

            lastPath.RemoveRange (i, lastPath.Count - i);

            lastKey = strKey;

            var node = i == 0 ? root : lastPath [i - 1];

            while (i < strKey.Length)
            {
                node = node.GetOrAddEdge (strKey [i]);
                lastPath.Add(node);
                ++i;
            }

            node.Payload = value;
        }

        private static void DoInsert (Node<TPayload> node, IEnumerable<char> key, TPayload value)
        {
            foreach (char c in key)
            {
                node = node.GetOrAddEdge (c);
            }

            node.Payload = value;
        }

        public bool TryGetValue (IEnumerable<char> key, out TPayload value)
        {
            value = default (TPayload);

            var node = root;

            foreach (char c in key)
            {
                node = node.GetChild (c);

                if (node == null) return false;
            }

            value = node.Payload;

            return node.HasPayload;
        }

        public Dawg <TPayload> BuildDawg ()
        {
            LevelBuilder <TPayload>.BuildLevelsExcludingRoot (root);

            return new Dawg<TPayload>(new OldDawg <TPayload> (root));
        }

        public Dawg<TPayload> BuildYaleDawg()
        {
            var dawg = BuildDawg();

            BuiltinTypeIO.Writers.TryGetValue(typeof(TPayload), out object writer);

            if (writer == null)
            {
                throw new NotImplementedException("BuildYaleDawg only works for built-in types currently.");
            }

            var memoryStream = new MemoryStream();

#pragma warning disable 612,618
            dawg.SaveAsYaleDawg(memoryStream, (Action<BinaryWriter, TPayload>) writer);
#pragma warning restore 612,618

            memoryStream.Position = 0;

            var rehydrated = Dawg<TPayload>.Load(memoryStream);

            return rehydrated;
        }
    }
}