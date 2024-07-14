﻿using System;
using System.IO;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.ViewModels;
using NitroxModel;
using NitroxModel.Discovery.Models;
using NitroxModel.Logger;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;

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

            IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(gameInstallDir);
            if (platform?.Platform == Platform.STEAM)
            {
                string gameVersionFile = Path.Combine(gameInstallDir, GameInfo.Subnautica.DataFolder, "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
                if (int.TryParse(await File.ReadAllTextAsync(gameVersionFile), out int gameVersion) && gameVersion <= 68598)
                {
                    if (dialogService != null)
                    {
                        await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                        {
                            model.Title = "Legacy Game Detected";
                            model.Description = $"Nitrox does not support the legacy version of Subnautica. Please update your game to the latest version to run the Subnautica with Nitrox.{Environment.NewLine}{Environment.NewLine}Version file location:{Environment.NewLine}{gameVersionFile}";
                            model.ButtonOptions = ButtonOptions.Ok;
                        });
                    }
                    return true;
                }
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
    public static bool IsGameRunning(GameInfo game)
    {
#if RELEASE
        if (NitroxModel.Platforms.OS.Shared.ProcessEx.ProcessExists(game.Name))
        {
            LauncherNotifier.Warning("An instance of Subnautica is already running");
            return true;
        }
#endif
        return false;
    }
}
