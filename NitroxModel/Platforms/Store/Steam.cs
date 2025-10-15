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
            _ => false
        };

    public async Task<ProcessEx?> StartPlatformAsync()
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
        ProcessEx process = ProcessEx.From(new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(exe) ?? Directory.GetCurrentDirectory(),
            FileName = launchExe,
            WindowStyle = ProcessWindowStyle.Minimized,
            UseShellExecute = true,
            Arguments = launchArgs
        });
        if (process is not { IsRunning: true })
        {
            process?.Dispose();
            return null;
        }

        steam = process;
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

    public string? GetExeFile()
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

    public async Task<ProcessEx?> StartGameAsync(string pathToGameExe, string launchArguments, int steamAppId, bool skipSteam, bool bigPictureMode)
    {
        // Handle Big Picture mode launch - this ensures Steam is running with Big Picture activated
        if (bigPictureMode)
        {
            await LaunchBigPictureInterfaceAsync();
        }
        else
        {
            // For regular mode, ensure Steam is running normally
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
        }

        return ProcessEx.From(CreateSteamGameStartInfo(pathToGameExe, GetExeFile(), launchArguments, steamAppId, skipSteam, bigPictureMode));
    }

    private async Task LaunchBigPictureInterfaceAsync()
    {
        // First ensure Steam is running
        ProcessEx? existingSteam = ProcessEx.GetFirstProcess(SteamProcessName);
        if (existingSteam == null)
        {
            Log.Info("Big Picture Mode: Steam not running, starting Steam...");
            string? steamExe = GetExeFile();
            if (steamExe == null)
            {
                throw new Exception("Could not find Steam executable for Big Picture mode launch");
            }

            // Start Steam normally first
            ProcessEx steam = ProcessEx.From(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(steamExe) ?? Directory.GetCurrentDirectory(),
                FileName = steamExe,
                WindowStyle = ProcessWindowStyle.Minimized,
                UseShellExecute = true,
                Arguments = "-silent" // Don't show Steam window
            });

            if (steam == null || !steam.IsRunning)
            {
                throw new Exception("Failed to start Steam for Big Picture mode");
            }

            // Wait for Steam to be ready
            using CancellationTokenSource steamReadyCts = new(TimeSpan.FromSeconds(30));
            try
            {
                DateTime consoleLogFileLastWrite = GetSteamConsoleLogLastWrite(steamExe);
                while (consoleLogFileLastWrite == GetSteamConsoleLogLastWrite(steamExe) && !steamReadyCts.IsCancellationRequested)
                {
                    await Task.Delay(250, steamReadyCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        string? steamExePath = GetExeFile();
        if (steamExePath == null)
        {
            throw new Exception("Could not find Steam executable for Big Picture mode launch");
        }

        // Try Steam protocol URL first (preferred method)
        Process? proc = Process.Start(new ProcessStartInfo
        {
            FileName = "steam://open/bigpicture",
            UseShellExecute = true
        });

        if (proc != null)
        {
            Log.Info("Big Picture Mode: Steam protocol activation successful");
            // Wait for Big Picture to start loading before launching the game.
            // This prevents focus stealing issues where Big Picture loads after the game has started
            // and steals focus from the game, causing a jarring user experience.
            // The 1-second delay provides enough time for Big Picture to initialize while keeping launch times reasonable.
            // Use the same timeout pattern as the main Steam launch for consistency.
            using CancellationTokenSource bigPictureReadyCts = new(TimeSpan.FromSeconds(15));
            try
            {
                Log.Debug("Waiting for Big Picture to initialize...");
                await Task.Delay(1000, bigPictureReadyCts.Token); // Minimum wait time to allow Big Picture initialization
                Log.Debug("Big Picture initialization delay complete");
            }
            catch (OperationCanceledException)
            {
                Log.Warn("Big Picture initialization timed out");
            }
        }
        else
        {
            Log.Info("Big Picture Mode: Steam protocol failed, trying direct launch...");
            try
            {
                proc = Process.Start(new ProcessStartInfo
                {
                    FileName = steamExePath,
                    Arguments = "-bigpicture",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (proc != null)
                {
                    Log.Info("Big Picture Mode: Direct launch successful");
                    // Wait for Big Picture to start loading before launching the game.
                    // This prevents focus stealing issues where Big Picture loads after the game has started
                    // and steals focus from the game, causing a jarring user experience.
                    // The 1-second delay provides enough time for Big Picture to initialize while keeping launch times reasonable.
                    // Use the same timeout pattern as the main Steam launch for consistency.
                    using CancellationTokenSource bigPictureReadyCts = new(TimeSpan.FromSeconds(15));
                    try
                    {
                        Log.Debug("Waiting for Big Picture to initialize...");
                        await Task.Delay(1000, bigPictureReadyCts.Token); // Minimum wait time to allow Big Picture initialization
                        Log.Debug("Big Picture initialization delay complete");
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Warn("Big Picture initialization timed out");
                    }
                }
                else
                {
                    Log.Error("Big Picture Mode: Direct launch process failed to start");
                    throw new Exception("Failed to launch Steam Big Picture interface");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Big Picture Mode: Direct launch failed with exception: {ex.Message}");
                throw new Exception("Failed to launch Steam Big Picture interface", ex);
            }
        }
    }

    private static ProcessStartInfo CreateSteamGameStartInfo(string gameFilePath, string? steamExe, string args, int steamAppId, bool skipSteam, bool bigPictureMode = false)
    {
        if (steamExe == null)
        {
            throw new FileNotFoundException("Steam was not found on your machine.");
        }
        
        string steamPath = Path.GetDirectoryName(steamExe);
        if (steamPath == null)
        {
            throw new Exception("Steam was not found on your machine.");
        }
        // Start game through Steam so Steam Overlay loads properly. TODO: HACK - this way should be removed if we add a call SteamAPI_Init before Unity Engine shows graphics, see https://partner.steamgames.com/doc/features/overlay.
        if (!skipSteam)
        {
            args = $@"-applaunch {steamAppId} --nitrox ""{NitroxUser.LauncherPath}"" {args}";
            if (bigPictureMode)
            {
                // Keep Steam client minimized but active in background to maintain overlay functionality
                // -silent prevents Steam from stealing focus from Big Picture mode
                args = $"-silent {args}";
            }
            
            return new()
            {
                FileName = steamExe,
                Arguments = args
            };
        }

        // Start through game executable. This allows custom args so that VR mode can be on with Nitrox (Subnautica hard codes '-vrmode none' as default launch option and starting game through Steam from command line always uses default launch option).
        // This way allows for multiple instances to run, but also stops some Steam integrations from working (e.g. Steam Input, Steam Overlay).
        ProcessStartInfo result = new()
        {
            FileName = gameFilePath,
            Arguments = args,
            EnvironmentVariables =
            {
                [NitroxUser.LAUNCHER_PATH_ENV_KEY] = NitroxUser.LauncherPath,
                ["SteamGameId"] = steamAppId.ToString(),
                ["SteamAppId"] = steamAppId.ToString(), // Primary Steam API var
                ["STEAM_OVERLAY"] = "1", // Force enable Steam overlay
                ["ENABLE_VKBASALT"] = "0" // VKBasalt prevents Steam overlay from working
            }
        };
        // Start via Proton on Linux.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string compatdataPath = "";
            if (!string.IsNullOrEmpty(gameFilePath))
            {
                string[] pathComponents = gameFilePath.Split(Path.DirectorySeparatorChar);
                int steamAppsIndex = pathComponents.GetIndex("steamapps");
                if (steamAppsIndex != -1)
                {
                    string steamAppsPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathComponents, 0, steamAppsIndex + 1);
                    compatdataPath = Path.Combine(steamAppsPath, "compatdata", steamAppId.ToString());
                }
            }

            string sniperAppId = "1628350";
            string sniperRuntimePath = Path.Combine(GetLibraryPath(steamPath, sniperAppId), "steamapps", "common", "SteamLinuxRuntime_sniper");

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

            result.FileName = Path.Combine(sniperRuntimePath, "_v2-entry-point");
            result.Arguments = $" --verb=run -- \"{Path.Combine(protonPath, "proton")}\" run \"{gameFilePath}\" {args}";
            result.EnvironmentVariables.Add("STEAM_COMPAT_APP_ID", steamAppId.ToString());
            result.EnvironmentVariables.Add("WINEPREFIX", compatdataPath);
            result.EnvironmentVariables.Add("STEAM_COMPAT_CLIENT_INSTALL_PATH", steamPath);
            result.EnvironmentVariables.Add("STEAM_COMPAT_DATA_PATH", compatdataPath);
            result.EnvironmentVariables.Add("STEAM_OVERLAY_LINUX", "1"); // Enable Steam overlay and API for controller input and OSK support (Proton-specific)
        }

        return result;
        
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

        static string? GetProtonVersionFromConfigVdf(string configVdfFile, string appId)
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
    }

    private static DateTime GetSteamConsoleLogLastWrite(string steamExePath)
    {
        string steamLogsPath = steamExePath switch
        {
            not null when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steam", "logs"),
            not null when Path.GetDirectoryName(steamExePath) is { } steamPath => Path.Combine(steamPath, "logs"),
            _ => throw new FileNotFoundException("Failed to find Steam console log file")
        };
        return File.GetLastWriteTime(Path.Combine(steamLogsPath, "console_log.txt"));
    }
}
