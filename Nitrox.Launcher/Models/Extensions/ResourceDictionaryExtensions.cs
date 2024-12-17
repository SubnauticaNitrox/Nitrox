using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Extensions;

public static class ResourceDictionaryExtensions
{
    public static object GetResource(this IResourceDictionary resourceDictionary, string key)
    {
        if (!resourceDictionary.TryGetResource(key, null, out object value))
        {
            return null;
        }
        return value;
    }
}
