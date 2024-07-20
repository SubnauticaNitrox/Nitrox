using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSubnauticaLaunchArguments(this IKeyValueStore store, string defaultValue = "-vrmode none") => store == null ? defaultValue : store.GetValue("SubnauticaLaunchArguments", defaultValue);

    public static void SetSubnauticaLaunchArguments(this IKeyValueStore store, string value)
    {
        if (store == null)
        {
            return;
        }
        store.SetValue("SubnauticaLaunchArguments", value);
    }
}
