using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nitrox.Launcher.Models.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this Collection<T> collection, params IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }
}
