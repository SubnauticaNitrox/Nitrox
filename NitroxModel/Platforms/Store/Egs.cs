using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    /// <summary>
    ///     Epic games store.
    /// </summary>
    public sealed class Egs : IGamePlatform
    {
        private static Egs instance;
        public static Egs Instance => instance ??= new Egs();

        public bool OwnsGame(string gameDirectory)
        {
            return Directory.Exists(Path.Combine(gameDirectory, ".egstore"));
        }

        public string Name => "Epic Games Store";

        public async Task<ProcessEx> StartPlatformAsync()
        {
            await Task.CompletedTask; // Suppresses async-without-await warning - can be removed.
            throw new NotImplementedException(); // Not necessary to implement unless EGS gets a game SDK and respective game integrates it.
        }

        public string GetExeFile()
        {
            throw new NotImplementedException();
        }

        public async Task<ProcessEx> StartGameAsync(string pathToGameExe)
        {
            // Normally should call StartPlatformAsync first. But Subnautica will start without EGS.
            return await Task.FromResult(ProcessEx.Start(pathToGameExe,
                                                         new[] { ("NITROX_LAUNCHER_PATH", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)) },
                                                         Path.GetDirectoryName(pathToGameExe),
                                                         "-EpicPortal -vrmode none"));
        }
    }
}
