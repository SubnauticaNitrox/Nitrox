using System;
using System.IO;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

/// <summary>
///     Handles installing and removing Nitrox as a BepInEx plugin.
/// </summary>
public static class BepInExSetup
{
    public const string BEPINEX_DIR = "BepInEx";
    public const string NITROX_PLUGIN_NAME = "NitroxPatcher.dll";

    private const string PLUGINS_DIR = "plugins";

    public static string GetPluginsPath(string gameBasePath) =>
        Path.Combine(gameBasePath, BEPINEX_DIR, PLUGINS_DIR);

    /// <summary>
    ///     Returns true if BepInEx is installed in the game directory.
    /// </summary>
    public static bool IsInstalled(string gameBasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(gameBasePath, nameof(gameBasePath));
        return Directory.Exists(Path.Combine(gameBasePath, BEPINEX_DIR));
    }

    /// <summary>
    ///     Copies the Nitrox patcher DLL into the BepInEx plugins folder.
    /// </summary>
    public static void InstallNitroxPlugin(string gameBasePath, string nitroxPatcherSourcePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(gameBasePath, nameof(gameBasePath));
        ArgumentException.ThrowIfNullOrEmpty(nitroxPatcherSourcePath, nameof(nitroxPatcherSourcePath));

        string pluginsDir = GetPluginsPath(gameBasePath);
        Directory.CreateDirectory(pluginsDir);

        string destPath = Path.Combine(pluginsDir, NITROX_PLUGIN_NAME);
        File.Copy(nitroxPatcherSourcePath, destPath, overwrite: true);
        Log.Debug($"Installed Nitrox BepInEx plugin to: {destPath}");
    }

    /// <summary>
    ///     Removes the Nitrox patcher DLL from the BepInEx plugins folder.
    /// </summary>
    public static void RemoveNitroxPlugin(string gameBasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(gameBasePath, nameof(gameBasePath));

        string pluginPath = Path.Combine(GetPluginsPath(gameBasePath), NITROX_PLUGIN_NAME);
        if (File.Exists(pluginPath))
        {
            File.Delete(pluginPath);
            Log.Debug($"Removed Nitrox BepInEx plugin from: {pluginPath}");
        }
    }

    /// <summary>
    ///     Returns true if the Nitrox plugin is present in the BepInEx plugins folder.
    /// </summary>
    public static bool IsNitroxPluginInstalled(string gameBasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(gameBasePath, nameof(gameBasePath));
        return File.Exists(Path.Combine(GetPluginsPath(gameBasePath), NITROX_PLUGIN_NAME));
    }
}
