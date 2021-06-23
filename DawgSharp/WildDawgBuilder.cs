using System;
using System.Collections.Generic;
using System.Linq;

namespace DawgSharp
{
    static class WildDawgBuilder<TPayload>
    {
        public static TPayload GetPayload(Node<TPayload> node)
        {
            if (!node.HasChildren)
                return node.Payload;
            
            var distinct = node.Children.Values.Select(GetPayload).Distinct().ToList();
            if (distinct.Count == 1)
            {
                TPayload payload = distinct[0];
                if (!AreEq(payload, default) && (AreEq(node.Payload, default) || AreEq(node.Payload, payload)))
                {
                    node.Children.Clear();
                    node.Children.Add('*', new Node<TPayload> {Payload = payload});
                    return payload;
                }
            }

            return default;
        }

        private static bool AreEq(TPayload a, TPayload b) => 
            Comparer<TPayload>.Default.Compare(a, b) == 0;
    }
}