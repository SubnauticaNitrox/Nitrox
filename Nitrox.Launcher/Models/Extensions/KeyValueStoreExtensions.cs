using System;
using System.IO;
using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    public static string GetSavesFolderDir(this IKeyValueStore store) => store.GetValue("SavesFolderDir", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves"));
    public static string GetSubnauticaLaunchArguments(this IKeyValueStore store, string defaultValue = "-vrmode none") => store.GetValue("SubnauticaLaunchArguments", defaultValue);
    public static void SetSubnauticaLaunchArguments(this IKeyValueStore store, string value) => store.SetValue("SubnauticaLaunchArguments", value);
}
