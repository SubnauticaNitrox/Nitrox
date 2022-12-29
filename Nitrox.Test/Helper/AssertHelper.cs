using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nitrox.Test.Helper;

public static class AssertHelper
{
    public static void IsListEqual<TSource>(IList<TSource> first, IList<TSource> second, Action<TSource, TSource> assertComparer)
    {
        Assert.AreEqual(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            assertComparer(first[index], second[index]);
        }
    }

    public static void IsDictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
    {
        Assert.AreEqual(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            KeyValuePair<TKey, TValue> firstKeyValuePair = first.ElementAt(index);
            KeyValuePair<TKey, TValue> secondKeyValuePair = first.ElementAt(index);

            Assert.AreEqual(firstKeyValuePair.Key, secondKeyValuePair.Key);
            Assert.AreEqual(firstKeyValuePair.Value, secondKeyValuePair.Value);
        }
    }

    public static void IsDictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, Action<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>> assertComparer)
    {
        Assert.AreEqual(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            assertComparer(first.ElementAt(index), second.ElementAt(index));
        }
    }
}
