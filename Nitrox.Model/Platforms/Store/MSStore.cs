using System;
using System.IO;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public sealed class MSStore : IGamePlatform
{
    public string Name => "Microsoft Store";
    public Platform Platform => Platform.MICROSOFT;

    public bool OwnsGame(string gameDirectory)
    {
        bool isLocalAppData = Path.GetFullPath(gameDirectory).StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"), StringComparison.InvariantCultureIgnoreCase);
        return isLocalAppData || File.Exists(Path.Combine(gameDirectory, "appxmanifest.xml"));
    }

    public static async Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        return await Task.FromResult(
            ProcessEx.Start(
                @"C:\Windows\System32\cmd.exe",
                [(NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                @$"/C start /b {pathToGameExe} --nitrox ""{NitroxUser.LauncherPath}"" {launchArguments}",
                createWindow: false)
        );
    }
}
