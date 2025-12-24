using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ZLoggerFormattableExtensions
{
    public static bool TryGetProperty<T>(this IZLoggerEntry entry, [NotNullWhen(true)] out T? result) where T : class
    {
        if (entry.LogInfo.ScopeState is { } scope)
        {
            foreach (KeyValuePair<string, object?> keyValuePair in scope.Properties)
            {
                object value = keyValuePair.Value;
                if (value is T matchedValue)
                {
                    result = matchedValue;
                    return true;
                }
            }
        }
        result = null;
        return false;
    }
}
