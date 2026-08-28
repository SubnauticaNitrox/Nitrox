using Nitrox.Model.Helper;

namespace Nitrox.Model.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSavesPath(this IKeyValueStore? store)
    {
        string defaultPath = NitroxDirectory.SavesPath;
        if (store == null)
        {
            return defaultPath;
        }
        return store.GetValue("SavesFolderDir", defaultPath);
    }
}
