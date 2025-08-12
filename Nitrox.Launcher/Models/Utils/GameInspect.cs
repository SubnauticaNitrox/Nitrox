using System;
using System.IO;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

internal static class GameInspect
{
    /// <summary>
    ///     Check to ensure the Subnautica is not in legacy.
    /// </summary>
    public static async Task<bool> IsOutdatedGameAndNotify(string gameInstallDir, IDialogService dialogService = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameInstallDir);
#if SUBNAUTICA
            string gameVersionFile = Path.Combine(gameInstallDir, GameInfo.Subnautica.DataFolder, "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
            if (int.TryParse(await File.ReadAllTextAsync(gameVersionFile), out int gameVersion) && gameVersion <= 68598)
#elif BELOWZERO
            string gameVersionFile = Path.Combine(gameInstallDir, GameInfo.SubnauticaBelowZero.DataFolder, "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
            if (int.TryParse(await File.ReadAllTextAsync(gameVersionFile), out int gameVersion) && gameVersion <= 49370)
#endif
            {
                if (dialogService != null)
                {
                    await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                    {
                        model.Title = "Legacy Game Detected";
#if SUBNAUTICA
                        model.Description = $"Nitrox does not support the legacy version of {GameInfo.Subnautica.FullName}. Please update your game to the latest version to run {GameInfo.Subnautica.FullName} with Nitrox.{Environment.NewLine}{Environment.NewLine}Version file location:{Environment.NewLine}{gameVersionFile}";
#elif BELOWZERO
                        model.Description = $"Nitrox does not support the legacy version of {GameInfo.SubnauticaBelowZero.FullName}. Please update your game to the latest version to run {GameInfo.SubnauticaBelowZero.FullName} with Nitrox.{Environment.NewLine}{Environment.NewLine}Version file location:{Environment.NewLine}{gameVersionFile}";
#endif
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

    /// <summary>
    ///     Checks game is running and if it is, warns. Does nothing in development mode for debugging purposes.
    /// </summary>
    public static bool WarnIfGameProcessExists(GameInfo game)
    {
        if (!NitroxEnvironment.IsReleaseMode)
        {
            return false;
        }

        if (!ProcessEx.ProcessExists(game.Name))
        {
            return false;
        }
        
        LauncherNotifier.Warning($"An instance of {game.FullName} is already running");
        return true;
    }
}
