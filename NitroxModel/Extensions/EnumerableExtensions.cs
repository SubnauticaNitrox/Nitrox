using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxModel.Extensions;

public static class EnumerableExtensions
{
    /// <returns>
    ///     <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" /><br />
    ///     <see langword="true" /> if both IEnumerables are null.
    /// </returns>
    /// <remarks>
    ///     <see cref="ArgumentNullException" /> can't be thrown because of <paramref name="first" /> or
    ///     <paramref name="second" /> being null.
    /// </remarks>
    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
    public static bool SequenceEqualOrBothNull<TSource>(this IEnumerable<TSource>? first, IEnumerable<TSource>? second)
    {
        if (first != null && second != null)
        {
            return first.SequenceEqual(second);
        }
        return first == second;
    }
}
