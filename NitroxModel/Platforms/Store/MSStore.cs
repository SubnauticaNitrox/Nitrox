﻿using System;
using System.IO;
using System.Threading.Tasks;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    public sealed class MSStore : IGamePlatform
    {
        private static MSStore instance;
        public static MSStore Instance => instance ??= new MSStore();

        public string Name => "Microsoft Store";

        public bool OwnsGame(string gameDirectory)
        {
            bool isLocalAppData = Path.GetFullPath(gameDirectory).StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"), StringComparison.InvariantCultureIgnoreCase);
            return isLocalAppData;
        }

        public async Task<ProcessEx> StartPlatformAsync()
        {
            await Task.CompletedTask; // Suppresses async-without-await warning - can be removed.
            throw new NotImplementedException($"{Name} support is unavailable");
        }

        public string GetExeFile()
        {
            throw new NotImplementedException($"{Name} support is unavailable");
        }

        public async Task<ProcessEx> StartGameAsync(string pathToGameExe)
        {
            // TODO: Support MS Store again and run command (example, should use pathToGameExe argument):
            // return await Task.FromResult(ProcessEx.Start(null,
            //                                        new[] { ("NITROX_LAUNCHER_PATH", "") },
            //                                        commandLine: GameInfo.Subnautica.MsStoreStartUrl));
            await Task.CompletedTask; // Suppresses async-without-await warning - can be removed.
            throw new NotImplementedException($"{Name} support is unavailable");
        }
    }
}