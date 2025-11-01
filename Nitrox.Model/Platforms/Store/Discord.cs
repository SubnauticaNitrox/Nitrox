﻿using System.IO;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public sealed class Discord : IGamePlatform
{
    public string Name => nameof(Discord);
    public Platform Platform => Platform.DISCORD;

    public bool OwnsGame(string gameDirectory)
    {
        return File.Exists(Path.Combine(Directory.GetParent(gameDirectory)?.FullName ?? "..", "journal.sqlite"));
    }

    public static async Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        return await Task.FromResult(
            ProcessEx.Start(
                pathToGameExe,
                [(NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            )
        );
    }
}
