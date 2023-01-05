using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nitrox.Test.Helper;

public static class AssertHelper
{
    public static void IsListEqual<TSource>(IOrderedEnumerable<TSource> first, IOrderedEnumerable<TSource> second, Action<TSource, TSource> assertComparer)
    {
        Assert.IsNotNull(first);
        Assert.IsNotNull(second);

        List<TSource> firstList = first.ToList();
        List<TSource> secondList = second.ToList();

        Assert.AreEqual(firstList.Count, secondList.Count);

        for (int index = 0; index < firstList.Count; index++)
        {
            assertComparer(firstList[index], secondList[index]);
        }
    }

    public static void IsDictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
    {
        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.AreEqual(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            KeyValuePair<TKey, TValue> firstKeyValuePair = first.ElementAt(index);
            Assert.IsTrue(second.TryGetValue(firstKeyValuePair.Key, out TValue secondValue), $"Second dictionary didn't contain {firstKeyValuePair.Key}");
            Assert.AreEqual(firstKeyValuePair.Value, secondValue, $"Values didn't match with the same key: {firstKeyValuePair.Key}");
        }
    }

    public static void IsDictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, Action<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>> assertComparer)
    {
        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.AreEqual(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            KeyValuePair<TKey, TValue> firstKeyValuePair = first.ElementAt(index);
            Assert.IsTrue(second.TryGetValue(firstKeyValuePair.Key, out TValue secondValue), $"Second dictionary didn't contain {firstKeyValuePair.Key}");

            assertComparer(firstKeyValuePair, new KeyValuePair<TKey, TValue>(firstKeyValuePair.Key, secondValue));
        }
    }
}
