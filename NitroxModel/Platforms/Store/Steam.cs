using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows;
using NitroxModel.Platforms.Store.Exceptions;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public sealed class Steam : IGamePlatform
{
    private static Steam instance;
    public static Steam Instance => instance ??= new Steam();

    public string Name => nameof(Steam);
    public Platform Platform => Platform.STEAM;

    public bool OwnsGame(string gameDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (Directory.Exists(Path.Combine(gameDirectory, "Plugins", "steam_api.bundle")))
            {
                return true;
            }
        }
        else
        {
            if (File.Exists(Path.Combine(gameDirectory, GameInfo.Subnautica.DataFolder, "Plugins", "x86_64", "steam_api64.dll")))
            {
                return true;
            }
            if (File.Exists(Path.Combine(gameDirectory, GameInfo.Subnautica.DataFolder, "Plugins", "steam_api64.dll")))
            {
                return true;
            }
        }
        return false;
    }

    public async Task<ProcessEx> StartPlatformAsync()
    {
        // If steam is already running, do not start it.
        // TODO: fix this for macos p => p.MainModuleDirectory != null && File.Exists(Path.Combine(p.MainModuleDirectory, "steamclient.dll"))
        ProcessEx steam = ProcessEx.GetFirstProcess("steam");
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

        // Wait for steam to write to its log file, which indicates it's ready to start games.
        using CancellationTokenSource steamReadyCts = new(TimeSpan.FromSeconds(30));
        try
        {
            DateTime consoleLogFileLastWrite = GetSteamConsoleLogLastWrite(Path.GetDirectoryName(exe));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await RegistryEx.CompareAsync<int>(@"SOFTWARE\Valve\Steam\ActiveProcess\ActiveUser",
                                                   v => v > 0,
                                                   steamReadyCts.Token);
            }
            while (consoleLogFileLastWrite == GetSteamConsoleLogLastWrite(Path.GetDirectoryName(exe)) && !steamReadyCts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(250, steamReadyCts.Token);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }

        return steam;
    }

    public string GetExeFile()
    {
        string exe = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            exe = Path.Combine(RegistryEx.Read(@"SOFTWARE\Valve\Steam\SteamPath", ""), "steam.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steam", "Steam.AppBundle", "Steam", "Contents", "MacOS", "steam_osx");
        }

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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ProcessEx.Start(
                pathToGameExe,
                new[] { ("SteamGameId", steamAppId.ToString()), ("SteamAppID", steamAppId.ToString()), (NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath) },
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            );
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {

            string compatdatapath = "";

            if (!string.IsNullOrEmpty(pathToGameExe))
            {
                string[] pathComponents = pathToGameExe.Split(Path.DirectorySeparatorChar);

                int steamappsIndex = Array.IndexOf(pathComponents, "steamapps");

                if (steamappsIndex != -1)
                {
                    string steamappsPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathComponents, 0, steamappsIndex + 1);

                    compatdatapath = Path.Combine(steamappsPath, "compatdata", steamAppId.ToString());
                }
            }
            string SteamPath = Path.Combine("/home/", Environment.GetEnvironmentVariable("USER"), ".steam/steam");

            // support flatpak
            if (!Directory.Exists(SteamPath))
            {
                SteamPath = Path.Combine("/home/", Environment.GetEnvironmentVariable("USER"), ".var/app/com.valvesoftware.Steam/data/Steam/");
            }

            string libraryfolders = Path.Combine(SteamPath, "config", "libraryfolders.vdf");

            // function to get library path for given game id
            string GetLibraryPath(string steamPath, string gameId)
            {
                string libraryFoldersPath = Path.Combine(steamPath, "config", "libraryfolders.vdf");
                string content = File.ReadAllText(libraryFoldersPath);

                // Regex to match library folder entries
                var folderRegex = new Regex(@"""(\d+)""\s*\{[^}]*""path""\s*""([^""]+)""[^}]*""apps""\s*\{([^}]+)\}", RegexOptions.Singleline);
                var matches = folderRegex.Matches(content);

                foreach (Match match in matches)
                {
                    string path = match.Groups[2].Value;
                    string apps = match.Groups[3].Value;

                    // Check if the gameId exists in the apps section
                    if (Regex.IsMatch(apps, $@"""{gameId}""\s*""[^""]+"""))
                    {
                        return path;
                    }
                }

                return ""; // Return empty string if not found
            }


            string sniperappid = "1628350";
            string sniperruntimepath = Path.Combine(GetLibraryPath(SteamPath, sniperappid), "steamapps", "common", "SteamLinuxRuntime_sniper");


            string protonappid = "2805730"; // app id for proton 9. A graphical way to choose which proton version to use on linux should beb added
            string protonpath = Path.Combine(GetLibraryPath(SteamPath, protonappid), "steamapps", "common");

            // get all folders here and choose the one that starts with Proton 9.0
            string protonfolder = null;
            foreach (string dir in Directory.GetDirectories(protonpath))
            {
                if (dir.Contains("Proton 9.0")) // this should be changed to a more robust way to get the proton version
                {
                    protonfolder = dir;
                    break;
                }
            }

            string gamelibrary = GetLibraryPath(SteamPath, steamAppId.ToString());

            string launchargs = $" --verb=waitforexitandrun -- \"{Path.Combine(protonfolder, "proton")}\" waitforexitandrun \"{pathToGameExe}\" {launchArguments}";


            ProcessStartInfo startInfo = new()
            {
                FileName = Path.Combine(sniperruntimepath, "_v2-entry-point"),
                Arguments = launchargs,
                WorkingDirectory = Path.GetDirectoryName(pathToGameExe) ?? "",
                UseShellExecute = false,
                Environment =
                {
                    [NitroxUser.LAUNCHER_PATH_ENV_KEY] = NitroxUser.LauncherPath,
                    ["SteamGameId"] = steamAppId.ToString(),
                    ["SteamAppID"] = steamAppId.ToString(),
                    ["STEAM_COMPAT_APP_ID"] = steamAppId.ToString(),
                    ["WINEPREFIX"] = compatdatapath,
                    ["STEAM_COMPAT_CLIENT_INSTALL_PATH"] = SteamPath,
                    ["STEAM_COMPAT_DATA_PATH"] = compatdatapath,





                }
            };
            return new ProcessEx(Process.Start(startInfo));

        }
        else
        {

            ProcessStartInfo startInfo = new()
            {
                FileName = pathToGameExe,
                WorkingDirectory = Path.GetDirectoryName(pathToGameExe) ?? "",
                UseShellExecute = false,
                Environment =
                {
                    [NitroxUser.LAUNCHER_PATH_ENV_KEY] = NitroxUser.LauncherPath,
                    ["SteamGameId"] = steamAppId.ToString(),
                    ["SteamAppID"] = steamAppId.ToString()
                }
            };
            Console.WriteLine($"Starting game with arguments: {startInfo.FileName} {launchArguments}");
            return new ProcessEx(Process.Start(startInfo));
        }
    }

    private DateTime GetSteamConsoleLogLastWrite(string exePath) => File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(exePath), "logs", "console_log.txt"));
}
