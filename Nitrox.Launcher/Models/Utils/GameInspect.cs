using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels;
using Nitrox.Model.Core;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

internal static class GameInspect
{
    /// <summary>
    ///     Check to ensure the Subnautica is not in legacy.
    /// </summary>
    public static async Task<bool> IsOutdatedGameAndNotify(string gameInstallPath, DialogService? dialogService = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameInstallPath);

            string dataPath = GetGameDataPath(gameInstallPath);
            string gameVersionFile = Path.Combine(dataPath, "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
            if (int.TryParse(await File.ReadAllTextAsync(gameVersionFile), out int gameVersion) && gameVersion < NitroxEnvironment.GameMinimumVersion)
            {
                if (dialogService != null)
                {
                    await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                    {
                        model.Title = "Outdated Game Detected";
                        model.Description =
                            $"Nitrox does not support the older {GameInfo.Subnautica.FullName} game version of {gameVersion}. Please update your game to the latest version.{Environment.NewLine}{Environment.NewLine}Minimum game version: {NitroxEnvironment.GameMinimumVersion}{Environment.NewLine}Version file location:{Environment.NewLine}{gameVersionFile}{Environment.NewLine}";
                        model.ButtonOptions = ButtonOptions.Ok;
                    });
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while checking game version:");
            LauncherNotifier.Debug(ex.Message);
            // On error: ignore and assume it's not outdated in case of unforeseen changes. We don't want to block users.
            return false;
        }

        return false;
    }

    private static string GetGameDataPath(string gameInstallPath)
    {
        if (GameInstallationHelper.TryGetGameInstallation(gameInstallPath, GameInfo.Subnautica, out GameInstallationLayout layout))
        {
            return Path.GetDirectoryName(layout.ManagedPath) ?? Path.Combine(layout.RootPath, GameInfo.Subnautica.DataFolder);
        }

        return Path.Combine(gameInstallPath, GameInfo.Subnautica.DataFolder);
    }

    /// <summary>
    ///     Checks game is running and if it is, warns. Does nothing in development mode for debugging purposes.
    /// </summary>
    public static bool WarnIfGameProcessExists(GameInfo game)
    {
        if (!ProcessEx.ProcessExists(game.ExeName))
        {
            return false;
        }
        LauncherNotifier.Warning($"An instance of {game.FullName} is already running");
        return true;
    }

    public static void WarnIfBepInExMods(string gamePath)
    {
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            return;
        }
        string bepRoot = Path.Combine(gamePath, "BepInEx");
        if (!Directory.Exists(bepRoot))
        {
            return;
        }

        int modDllCount = GetDllPaths(Path.Combine(bepRoot, "plugins")).Count();
        modDllCount += GetDllPaths(Path.Combine(bepRoot, "patchers")).Count();
        if (modDllCount > 0)
        {
            Log.Warn($"BepInEx plugins detected: {modDllCount}");
            LauncherNotifier.Warning($"BepInEx mod(s) were detected ({modDllCount}). Nitrox multiplayer does not support mods and they may cause instability.");
        }

        static IEnumerable<string> GetDllPaths(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories);
            }
            catch (IOException)
            {
                return [];
            }
        }
    }
}
