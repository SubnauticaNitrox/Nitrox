using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    public sealed class Discord : IGamePlatform
    {
        private static Discord instance;
        public static Discord Instance => instance ??= new Discord();

        public string Name => nameof(Discord);

        public bool OwnsGame(string gameDirectory)
        {
            return File.Exists(Path.Combine(Directory.GetParent(gameDirectory)?.FullName ?? "", "journal.sqlite"));
        }

        public async Task<ProcessEx> StartPlatformAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public string GetExeFile()
        {
            throw new NotImplementedException();
        }

        public async Task<ProcessEx> StartGameAsync(string pathToGameExe)
        {
            return await Task.FromResult(ProcessEx.Start(pathToGameExe,
                                   new[] { ("NITROX_LAUNCHER_PATH", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)) },
                                   Path.GetDirectoryName(pathToGameExe))
            );
        }
    }
}