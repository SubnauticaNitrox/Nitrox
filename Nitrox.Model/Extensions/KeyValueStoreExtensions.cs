using System.IO;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSavesFolderDir(this IKeyValueStore? store)
    {
        if (store == null)
        {
            return Path.Combine(NitroxUser.AppDataPath, "saves");
        }
        return store.GetValue("SavesFolderDir", Path.Combine(NitroxUser.AppDataPath, "saves"));
    }
}
