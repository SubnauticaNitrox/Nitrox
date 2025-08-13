using System.Collections.Generic;

namespace NitroxModel;

public static class DotnetCompatibilityExtension
{
#if NETFRAMEWORK
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (key == null || dict.ContainsKey(key))
        {
            return false;
        }

        dict.Add(key, value);
        return true;
    }
#endif
}
