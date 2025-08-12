using System;
using System.IO;
using System.Threading.Tasks;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public sealed class Discord : IGamePlatform
{
    public string Name => nameof(Discord);
    public Platform Platform => Platform.DISCORD;

    public bool OwnsGame(string gameDirectory)
    {
        return File.Exists(Path.Combine(Directory.GetParent(gameDirectory)?.FullName ?? "..", "journal.sqlite"));
    }

    public Task<ProcessEx> StartPlatformAsync()
    {
        throw new NotImplementedException();
    }

    public string GetExeFile()
    {
        throw new NotImplementedException();
    }

    public async Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
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
