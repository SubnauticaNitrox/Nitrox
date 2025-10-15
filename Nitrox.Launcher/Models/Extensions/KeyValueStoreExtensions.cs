using Nitrox.Model.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetLaunchArguments(this IKeyValueStore store, GameInfo gameInfo, string defaultValue = "-vrmode none") => store.GetValue($"{gameInfo.Name}LaunchArguments", defaultValue);

    public static void SetLaunchArguments(this IKeyValueStore store, GameInfo gameInfo, string value) => store.SetValue($"{gameInfo.Name}LaunchArguments", value);

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
