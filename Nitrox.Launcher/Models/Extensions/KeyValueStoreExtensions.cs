using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSubnauticaLaunchArguments(this IKeyValueStore store, string defaultValue = "-vrmode none") => store.GetValue("SubnauticaLaunchArguments", defaultValue);
    public static void SetSubnauticaLaunchArguments(this IKeyValueStore store, string value) => store.SetValue("SubnauticaLaunchArguments", value);
}
