using System;
using System.Collections.Generic;

namespace NitroxModel.Helper;

public static class LinqExtensions
{
    /// <summary>
    ///     Returns the items until the predicate stops matching, then includes the next non-matching result as well.
    /// </summary>
    /// <param name="source">Input enumerable.</param>
    /// <param name="predicate">Predicate to match against the items in the enumerable.</param>
    /// <typeparam name="T">Type of items to return.</typeparam>
    /// <returns></returns>
    public static IEnumerable<T> TakeUntilInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (T item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
            else
            {
                // Also self
                yield return item;
                yield break;
            }
        }
    }
}
