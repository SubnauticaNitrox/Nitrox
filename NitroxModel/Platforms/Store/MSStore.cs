using System;
using System.IO;
using System.Threading.Tasks;
using NitroxModel.Discovery;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    public sealed class MSStore : IGamePlatform
    {
        private static MSStore instance;
        public static MSStore Instance => instance ??= new MSStore();

        public string Name => "Microsoft Store";
        public Platform platform => Platform.MICROSOFT;

        public bool OwnsGame(string gameDirectory)
        {
            bool isLocalAppData = Path.GetFullPath(gameDirectory).StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"), StringComparison.InvariantCultureIgnoreCase);
            return isLocalAppData || File.Exists(Path.Combine(gameDirectory, "appxmanifest.xml"));
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
            //                                        new[] { (NitroxUser.LAUNCHER_PATH_ENV_KEY, "") },
            //                                        commandLine: GameInfo.Subnautica.MsStoreStartUrl));
            await Task.CompletedTask; // Suppresses async-without-await warning - can be removed.
            throw new NotImplementedException($"{Name} support is unavailable");
        }
    }
}
