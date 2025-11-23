using System;
using System.Buffers;
using System.Collections.Generic;

namespace Nitrox.Model.Extensions;

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

#if NETFRAMEWORK
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (key == null || dict.ContainsKey(key))
        {
            return false;
        }

        dict.Add(key, value);
        return true;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return dictionary.TryGetValue(key, out TValue obj) ? obj : defaultValue;
    }
#endif
}
