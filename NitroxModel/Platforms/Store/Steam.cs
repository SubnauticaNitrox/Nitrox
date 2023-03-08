using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows.Internal;
using NitroxModel.Platforms.Store.Exceptions;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    public sealed class Steam : IGamePlatform
    {
        private static Steam instance;
        public static Steam Instance => instance ??= new Steam();

        public string Name => nameof(Steam);
        public Platform Platform => Platform.STEAM;

        public bool OwnsGame(string gameDirectory)
        {
            return File.Exists(Path.Combine(gameDirectory, "Subnautica_Data", "Plugins", "x86_64", "steam_api64.dll"));
        }

        public async Task<ProcessEx> StartPlatformAsync()
        {
            // If steam is already running, do not start it.
            ProcessEx steam = ProcessEx.GetFirstProcess("steam", p => p.MainModuleDirectory != null && File.Exists(Path.Combine(p.MainModuleDirectory, "steamclient.dll")));
            if (steam != null)
            {
                return steam;
            }

            // Steam is not running, start it.
            string exe = GetExeFile();
            if (exe == null)
            {
                return null;
            }
            steam = new ProcessEx(Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? Directory.GetCurrentDirectory(),
                FileName = exe,
                WindowStyle = ProcessWindowStyle.Minimized,
                Arguments = "-silent" // Don't show Steam window
            }));

            // Wait for Steam to get ready. Steam will update the PID and set the ActiveUser to 0 while starting. Once UI is loaded it will update ActiveUser to > 0 value.
            await RegistryEx.CompareAsync<int>(@"SOFTWARE\Valve\Steam\ActiveProcess\pid",
                                               v => v == steam.Id,
                                               TimeSpan.FromSeconds(45));
            await RegistryEx.CompareAsync<int>(@"SOFTWARE\Valve\Steam\ActiveProcess\ActiveUser",
                                               v => v == 0,
                                               TimeSpan.FromSeconds(20));
            await RegistryEx.CompareAsync<int>(@"SOFTWARE\Valve\Steam\ActiveProcess\ActiveUser",
                                               v => v > 0,
                                               TimeSpan.FromSeconds(20));
            return steam;
        }

        public string GetExeFile()
        {
            string steamPath = RegistryEx.Read(@"SOFTWARE\Valve\Steam\SteamPath", "");
            string exe = Path.Combine(steamPath, "steam.exe");
            return File.Exists(exe) ? Path.GetFullPath(exe) : null;
        }

        public async Task<ProcessEx> StartGameAsync(string pathToGameExe, int steamAppId, string launchArguments)
        {
            try
            {
                using ProcessEx steam = await StartPlatformAsync();
                if (steam == null)
                {
                    throw new PlatformException(Instance, "Steam is not running and could not be found.");
                }
            }
            catch (OperationCanceledException ex)
            {
                throw new PlatformException(Instance, "Timeout reached while waiting for platform to start. Try again once platform has finished loading.", ex);
            }

            return ProcessEx.Start(
                    pathToGameExe,
                    new[] { ("SteamGameId", steamAppId.ToString()), ("SteamAppID", steamAppId.ToString()), (NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath) },
                    Path.GetDirectoryName(pathToGameExe),
                    launchArguments
            );
        }
    }
}
