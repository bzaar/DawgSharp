using System.Collections.Generic;
using System.Linq;

namespace DawgSharp;

public class SequenceEqualityComparer<T> : IEqualityComparer<IList<T>>
{
    private readonly IEqualityComparer<T> elementComparer;

    public SequenceEqualityComparer(IEqualityComparer<T> elementComparer = null)
    {
        this.elementComparer = elementComparer ?? EqualityComparer<T>.Default;
    }
        
    public bool Equals(IList<T> x, IList<T> y)
    {
        return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y, elementComparer));
    }

    public int GetHashCode(IList<T> obj)
    {
        if (obj == null) return 0;
            
        // Will not throw an OverflowException
        unchecked
        {
            return obj.Where(e => e != null).Select(elementComparer.GetHashCode).Aggregate(17, (a, b) => 23 * a + b);
        }
    }
}