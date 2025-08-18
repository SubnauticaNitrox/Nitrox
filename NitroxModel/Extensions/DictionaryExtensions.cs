using System;
using System.Buffers;
using System.Collections.Generic;

namespace NitroxModel.Extensions;

public static class DictionaryExtensions
{
    public static void RemoveWhere<TKey, TValue, TParameter>(this IDictionary<TKey, TValue> dictionary, TParameter extraParameter, Func<TValue, TParameter, bool> predicate)
    {
        int toRemoveIndex = 0;
        TKey[] toRemove = ArrayPool<TKey>.Shared.Rent(dictionary.Count);
        try
        {
            foreach (KeyValuePair<TKey, TValue> item in dictionary)
            {
                if (predicate.Invoke(item.Value, extraParameter))
                {
                    toRemove[toRemoveIndex++] = item.Key;
                }
            }
            for (int i = 0; i < toRemoveIndex; i++)
            {
                dictionary.Remove(toRemove[i]);
            }
        }
        finally
        {
            ArrayPool<TKey>.Shared.Return(toRemove, true);
        }
    }
}
