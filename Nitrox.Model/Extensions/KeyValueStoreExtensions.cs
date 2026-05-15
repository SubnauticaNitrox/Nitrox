using System.IO;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSavesFolderDir(this IKeyValueStore? store)
    {
        string defaultPath = Path.Combine(NitroxUser.GetSpecializedDirectory(NitroxUser.SpecializedDirectory.Data), "saves");
        if (store == null)
        {
            return defaultPath;
        }
        return store.GetValue("SavesFolderDir", defaultPath);
    }
}
