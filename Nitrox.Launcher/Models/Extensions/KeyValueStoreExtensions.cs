using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSubnauticaLaunchArguments(this IKeyValueStore store, string defaultValue = "-vrmode none") => store.GetValue("SubnauticaLaunchArguments", defaultValue);

    public static void SetSubnauticaLaunchArguments(this IKeyValueStore store, string value)
    {
        store.SetValue("SubnauticaLaunchArguments", value);
    }
    
    public static bool GetIsLightModeEnabled(this IKeyValueStore store, bool defaultValue = false) => store.GetValue("IsLightModeEnabled", defaultValue);

    public static void SetIsLightModeEnabled(this IKeyValueStore store, bool value)
    {
        store.SetValue("IsLightModeEnabled", value);
    }
    
    public static bool GetIsMultipleGameInstancesAllowed(this IKeyValueStore store, bool defaultValue = false) => store.GetValue("IsMultipleGameInstancesAllowed", defaultValue);

    public static void SetIsMultipleGameInstancesAllowed(this IKeyValueStore store, bool value)
    {
        store.SetValue("IsMultipleGameInstancesAllowed", value);
    }
}
