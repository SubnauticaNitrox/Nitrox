﻿using System;
using System.IO;
using System.Threading.Tasks;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public sealed class MSStore : IGamePlatform
{
    public string Name => "Microsoft Store";
    public Platform Platform => Platform.MICROSOFT;

    public bool OwnsGame(string gameDirectory)
    {
        bool isLocalAppData = Path.GetFullPath(gameDirectory).StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"), StringComparison.InvariantCultureIgnoreCase);
        return isLocalAppData || File.Exists(Path.Combine(gameDirectory, "appxmanifest.xml"));
    }

    public Task<ProcessEx> StartPlatformAsync()
    {
        throw new NotImplementedException("Unnecessary to start platform");
    }

    public string GetExeFile()
    {
        throw new NotImplementedException("Unnecessary to get platform executable");
    }

    public async Task<ProcessEx> StartGameAsync(string pathToGameExe, string subnauticaLaunchArguments = "")
    {
        return await Task.FromResult(
            ProcessEx.Start(
                @"C:\Windows\System32\cmd.exe",
                [(NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                @$"/C start /b {pathToGameExe} --nitrox ""{NitroxUser.LauncherPath}"" {subnauticaLaunchArguments}",
                createWindow: false)
        );
    }
}
