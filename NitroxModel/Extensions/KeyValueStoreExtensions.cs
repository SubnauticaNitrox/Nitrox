using System.IO;
using NitroxModel.Helper;

namespace NitroxModel.Extensions;

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
