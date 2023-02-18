using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxModel;

public static class Extensions
{
    public static TAttribute GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);

        return type.GetField(name)
                   .GetCustomAttributes(false)
                   .OfType<TAttribute>()
                   .SingleOrDefault();
    }

    /// <summary>
    ///     Removes all items from the list when the predicate returns true.
    /// </summary>
    /// <param name="list">The list to remove items from.</param>
    /// <param name="extraParameter">An extra parameter to supply to the predicate.</param>
    /// <param name="predicate">The predicate that tests each item in the list for removal.</param>
    public static void RemoveAllFast<TItem, TParameter>(this IList<TItem> list, TParameter extraParameter, Func<TItem, TParameter, bool> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            TItem item = list[i];
            if (predicate.Invoke(item, extraParameter))
            {
                // Optimization for Unity mono: swap item to end and remove it. This reduces GC pressure for resizing arrays.
                list[i] = list[^1];
                list.RemoveAt(list.Count - 1);
            }
        }
    }

    public static int GetIndex<T>(this T[] list, T itemToFind) => Array.IndexOf(list, itemToFind);
}
