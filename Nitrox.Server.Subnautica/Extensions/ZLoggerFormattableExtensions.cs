using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ZLoggerFormattableExtensions
{
    public static bool TryGetProperty<T>(this IZLoggerEntry entry, out T result, T? equality = default)
    {
        if (entry.LogInfo.ScopeState is { } scope)
        {
            foreach (KeyValuePair<string, object?> keyValuePair in scope.Properties)
            {
                object value = keyValuePair.Value;
                if ((typeof(T).IsAssignableFrom(value.GetType()) && Equals(equality, default(T))) || value.Equals(equality))
                {
                    result = (T)value;
                    return true;
                }
            }
        }
        result = default;
        return false;
    }
}
