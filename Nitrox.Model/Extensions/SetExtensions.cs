using System;
using System.Buffers;
using System.Collections.Generic;

namespace Nitrox.Model.Extensions;

public static class SetExtensions
{
    public static void RemoveWhere<TParam, T>(this ISet<T> self, TParam parameter, Func<TParam, T, bool> predicate)
    {
        int toRemoveIndex = 0;
        T[] toRemove = ArrayPool<T>.Shared.Rent(self.Count);
        try
        {
            foreach (T item in self)
            {
                if (predicate(parameter, item))
                {
                    toRemove[toRemoveIndex++] = item;
                }
            }
            for (int i = 0; i < toRemoveIndex; i++)
            {
                self.Remove(toRemove[i]);
            }
        }
        finally
        {
            ArrayPool<T>.Shared.Return(toRemove, true);
        }
    }
}
