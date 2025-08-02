﻿using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

internal static class GameInspect
{
    /// <summary>
    ///     Check to ensure the Subnautica is not in legacy.
    /// </summary>
    public static async Task<bool> IsOutdatedGameAndNotify(string gameInstallDir, DialogService? dialogService = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameInstallDir);
            
            string gameVersionFile = Path.Combine(gameInstallDir, GameInfo.Subnautica.DataFolder, "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
            if (int.TryParse(await File.ReadAllTextAsync(gameVersionFile), out int gameVersion) && gameVersion <= 68598)
            {
                if (dialogService != null)
                {
                    await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                    {
                        model.Title = "Legacy Game Detected";
                        model.Description =
                            $"Nitrox does not support the legacy version of {GameInfo.Subnautica.FullName}. Please update your game to the latest version to run {GameInfo.Subnautica.FullName} with Nitrox.{Environment.NewLine}{Environment.NewLine}Version file location:{Environment.NewLine}{gameVersionFile}";
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
        if (!ProcessEx.ProcessExists(game.ExeName))
        {
            return false;
        }
        
        LauncherNotifier.Warning($"An instance of {game.FullName} is already running");
        return true;
    }
}
