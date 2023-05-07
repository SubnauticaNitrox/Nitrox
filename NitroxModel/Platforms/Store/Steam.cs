using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows;
using NitroxModel.Platforms.Store.Exceptions;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public sealed class Steam : IGamePlatform
{
    public string Name => nameof(Steam);
    public Platform Platform => Platform.STEAM;

    private string SteamProcessName => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "steam_osx" : "steam";

    public bool OwnsGame(string gameRootPath) =>
        gameRootPath switch
        {
            not null when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => Directory.Exists(Path.Combine(gameRootPath, "Plugins", "steam_api.bundle")),
            not null when File.Exists(Path.Combine(gameRootPath, GameInfo.Subnautica.DataFolder, "Plugins", "x86_64", "steam_api64.dll")) => true,
            not null when File.Exists(Path.Combine(gameRootPath, GameInfo.Subnautica.DataFolder, "Plugins", "steam_api64.dll")) => true,
            not null when File.Exists(Path.Combine(gameRootPath, GameInfo.SubnauticaBelowZero.DataFolder, "Plugins", "x86_64", "steam_api64.dll")) => true,
            not null when File.Exists(Path.Combine(gameRootPath, GameInfo.SubnauticaBelowZero.DataFolder, "Plugins", "steam_api64.dll")) => true,
            _ => false
        };

    public async Task<ProcessEx> StartPlatformAsync()
    {
        // If steam is already running, do not start it.
        ProcessEx steam = ProcessEx.GetFirstProcess(SteamProcessName);
        if (steam is not null)
        {
            return steam;
        }

        // Steam is not running, start it.
        string exe = GetExeFile();
        if (exe is null)
        {
            return null;
        }

        string launchExe = exe;
        string launchArgs = "-silent"; // Don't show Steam window
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            launchExe = "/bin/sh";
            launchArgs = $@"-c ""nohup '{exe}' {launchArgs}"" &";
        }
        Stopwatch steamReadyStopwatch = Stopwatch.StartNew();
        Process process = Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(exe) ?? Directory.GetCurrentDirectory(),
            FileName = launchExe,
            WindowStyle = ProcessWindowStyle.Minimized,
            UseShellExecute = true,
            Arguments = launchArgs
        });

        if (process is not { HasExited: false })
        {
            return null;
        }

        steam = new ProcessEx(process);
        // Wait for steam to write to its log file, which indicates it's ready to start games.
        using CancellationTokenSource steamReadyCts = new(TimeSpan.FromSeconds(30));
        try
        {
            DateTime consoleLogFileLastWrite = GetSteamConsoleLogLastWrite(exe);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await RegistryEx.CompareWaitAsync<int>(@"SOFTWARE\Valve\Steam\ActiveProcess\ActiveUser",
                                                       v => v > 0,
                                                       steamReadyCts.Token);
            }
            Log.Debug("Waiting for Steam to get ready...");
            while (consoleLogFileLastWrite == GetSteamConsoleLogLastWrite(exe) && !steamReadyCts.IsCancellationRequested)
            {
                await Task.Delay(250, steamReadyCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        Log.Debug($"Steam wait result: {(steamReadyCts.IsCancellationRequested ? "timed out" : $"startup successful and took about {steamReadyStopwatch.Elapsed.TotalSeconds}s")}");

        return steam;
    }

    public string GetExeFile()
    {
        string steamExecutable = "";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            steamExecutable = Path.Combine(RegistryEx.Read(@"SOFTWARE\Valve\Steam\SteamPath", steamExecutable), "steam.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            steamExecutable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steam", "Steam.AppBundle", "Steam", "Contents", "MacOS", "steam_osx");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!Directory.Exists(userHomePath))
            {
                return null;
            }

            string steamPath = Path.Combine(userHomePath, ".steam", "steam");
            // support flatpak
            if (!Directory.Exists(steamPath))
            {
                steamPath = Path.Combine(userHomePath, ".var", "app", "com.valvesoftware.Steam", "data", "Steam");
            }

            steamExecutable = Path.Combine(steamPath, "steam.sh");
        }

        return File.Exists(steamExecutable) ? Path.GetFullPath(steamExecutable) : null;
    }

    public async Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments, int steamAppId)
    {
        try
        {
            using ProcessEx steam = await StartPlatformAsync();
            if (steam == null)
            {
                throw new GamePlatformException(this, "Platform is not running and could not be found.");
            }
        }
        catch (OperationCanceledException ex)
        {
            throw new GamePlatformException(this, "Timeout reached while waiting for platform to start. Try again once platform has finished loading.", ex);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
#if DEBUG // Needed to start multiple SN instances, but Steam Overlay doesn't work this way so only active for devs
            return ProcessEx.Start(
                pathToGameExe,
                [("SteamGameId", steamAppId.ToString()), ("SteamAppID", steamAppId.ToString()), (NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            );
#else
            return new ProcessEx(Process.Start(new ProcessStartInfo
            {
                FileName = GetExeFile(),
                Arguments = $"""-applaunch {steamAppId} --nitrox "{NitroxUser.LauncherPath}" {launchArguments}"""
            }));
#endif
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string steamPath = Path.GetDirectoryName(GetExeFile());
            return StartGameWithProton(steamPath, pathToGameExe, steamAppId, launchArguments);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ProcessEx.Start(
                pathToGameExe,
                [("SteamGameId", steamAppId.ToString()), ("SteamAppID", steamAppId.ToString()), (NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            );
        }

        throw new NotSupportedException("Your operating system is not supported by Nitrox");
    }

    private static ProcessEx StartGameWithProton(string steamPath, string pathToGameExe, int steamAppId, string launchArguments)
    {
        // function to get library path for given game id
        static string GetLibraryPath(string steamPath, string gameId)
        {
            string libraryFoldersPath = Path.Combine(steamPath, "config", "libraryfolders.vdf");
            string content = File.ReadAllText(libraryFoldersPath);

            // Regex to match library folder entries
            Regex folderRegex = new(@"""(\d+)""\s*\{[^}]*""path""\s*""([^""]+)""[^}]*""apps""\s*\{([^}]+)\}", RegexOptions.Singleline);
            MatchCollection matches = folderRegex.Matches(content);

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

        static List<string> GetAllLibraryPaths(string steamPath)
        {
            string libraryFoldersPath = Path.Combine(steamPath, "config", "libraryfolders.vdf");
            string content = File.ReadAllText(libraryFoldersPath);

            // Regex to match library folder entries
            Regex folderRegex = new(@"""(\d+)""\s*\{[^}]*""path""\s*""([^""]+)""", RegexOptions.Singleline);
            MatchCollection matches = folderRegex.Matches(content);

            List<string> libraryPaths = [];
            foreach (Match match in matches)
            {
                string path = match.Groups[2].Value.Replace("\\\\", "\\");
                if (Directory.Exists(Path.Combine(path, "steamapps", "common")))
                {
                    libraryPaths.Add(path);
                }
            }
            // Add the default Steam library path
            string defaultLibraryPath = Path.Combine(steamPath);
            if (!libraryPaths.Contains(defaultLibraryPath))
            {
                libraryPaths.Add(defaultLibraryPath);
            }

            return libraryPaths;
        }

        static string GetProtonVersionFromConfigVdf(string configVdfFile, string appId)
        {
            try
            {
                string fileContent = File.ReadAllText(configVdfFile);
                Match compatToolMatch = Regex.Match(fileContent, @"""CompatToolMapping""\s*{((?:\s*""\d+""[^{]+[^}]+})*)\s*}");

                if (compatToolMatch.Success)
                {
                    string compatToolMapping = compatToolMatch.Groups[1].Value;
                    string appIdPattern = $@"""{appId}""[^{{]*\{{[^}}]*""name""\s*""([^""]+)""";
                    Match appIdMatch = Regex.Match(compatToolMapping, appIdPattern);

                    if (appIdMatch.Success)
                    {
                        return appIdMatch.Groups[1].Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return null;
            }
        }

        string compatdataPath = "";
        if (!string.IsNullOrEmpty(pathToGameExe))
        {
            string[] pathComponents = pathToGameExe.Split(Path.DirectorySeparatorChar);
            int steamAppsIndex = pathComponents.GetIndex("steamapps");
            if (steamAppsIndex != -1)
            {
                string steamAppsPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathComponents, 0, steamAppsIndex + 1);
                compatdataPath = Path.Combine(steamAppsPath, "compatdata", steamAppId.ToString());
            }
        }

        string sniperappid = "1628350";
        string sniperruntimepath = Path.Combine(GetLibraryPath(steamPath, sniperappid), "steamapps", "common", "SteamLinuxRuntime_sniper");

        string protonPath = null;
        string protonRoot = Path.Combine(steamPath, "compatibilitytools.d");
        string protonVersion = GetProtonVersionFromConfigVdf(Path.Combine(steamPath, "config", "config.vdf"), steamAppId.ToString()) ?? "proton_9";
        bool isValveProton = protonVersion.StartsWith("proton_", StringComparison.OrdinalIgnoreCase);
        if (isValveProton)
        {
            int index = protonVersion.IndexOf("proton_", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                protonVersion = protonVersion[(index + "proton_".Length)..];
            }
            if (protonVersion == "experimental")
            {
                protonVersion = "-";
            }

            foreach (string path in GetAllLibraryPaths(steamPath))
            {
                foreach (string dir in Directory.EnumerateDirectories(Path.Combine(path, "steamapps", "common")))
                {
                    if (dir.Contains($"Proton {protonVersion}"))
                    {
                        protonPath = dir;
                        break;
                    }
                }
                if (protonPath != null)
                {
                    break;
                }
            }
        }
        else
        {
            protonPath = Path.Combine(protonRoot, protonVersion);
        }
        if (protonPath == null)
        {
            throw new Exception("Game is not using Proton. Please change game properties in Steam to use the Proton compatibility layer.");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = Path.Combine(sniperruntimepath, "_v2-entry-point"),
            Arguments = $" --verb=run -- \"{Path.Combine(protonPath, "proton")}\" run \"{pathToGameExe}\" {launchArguments}",
            WorkingDirectory = Path.GetDirectoryName(pathToGameExe) ?? "",
            UseShellExecute = false,
            Environment =
            {
                [NitroxUser.LAUNCHER_PATH_ENV_KEY] = NitroxUser.LauncherPath,
                ["SteamGameId"] = steamAppId.ToString(),
                ["SteamAppID"] = steamAppId.ToString(),
                ["STEAM_COMPAT_APP_ID"] = steamAppId.ToString(),
                ["WINEPREFIX"] = compatdataPath,
                ["STEAM_COMPAT_CLIENT_INSTALL_PATH"] = steamPath,
                ["STEAM_COMPAT_DATA_PATH"] = compatdataPath,
            }
        };
        return new ProcessEx(Process.Start(startInfo));
    }

    private static DateTime GetSteamConsoleLogLastWrite(string steamExePath)
    {
        string steamLogsPath = steamExePath switch
        {
            not null when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steam", "logs"),
            not null when Path.GetDirectoryName(steamExePath) is { } steamPath => Path.Combine(steamPath, "logs"),
            _ => ""
        };
        return File.GetLastWriteTime(Path.Combine(steamLogsPath, "console_log.txt"));
    }
}
