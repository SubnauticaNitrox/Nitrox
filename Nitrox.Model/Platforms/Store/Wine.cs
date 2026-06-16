using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store.Exceptions;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public sealed class Wine : IGamePlatform
{
    private const string WineExecutableOverrideEnvKey = "NITROX_WINE_EXE";
    private const string SteamStartupDelayMsEnvKey = "NITROX_WINE_STEAM_STARTUP_DELAY_MS";
    private const string DisableIpv6EnvKey = "NITROX_DISABLE_IPV6";
    private const string ClientConnectTimeoutMsEnvKey = "NITROX_CLIENT_CONNECT_TIMEOUT_MS";
    private const string LiteNetLibManualModeEnvKey = "NITROX_LITENETLIB_MANUAL_MODE";
    private const string LiteNetLibCrcFallbackEnvKey = "NITROX_LITENETLIB_CRC_FALLBACK";
    private const string LiteNetLibDisableNativeSocketsEnvKey = "NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS";
    private const int DefaultSteamStartupDelayMs = 10_000;
    private static readonly string SteamLaunchArguments = string.Join(" ",
    [
        "-noverifyfiles",
        "-nobootstrapupdate",
        "-skipinitialbootstrap",
        "-norepairfiles",
        "-overridepackageurl",
        "-vgui",
        "-noreactlogin",
        "-allosarches",
        "-cef-force-32bit",
        "-no-cef-sandbox",
        "-cef-disable-sandbox",
        "-cef-disable-gpu",
        "-cef-disable-gpu-sandbox"
    ]);

    public string Name => nameof(Wine);
    public Platform Platform => Platform.WINE;

    public bool OwnsGame(string gameDirectory)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
               GameInstallationHelper.IsWindowsGameLayout(gameDirectory, GameInfo.Subnautica);
    }

    public static async Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        string? wineExecutable = FindWineExecutable();
        if (wineExecutable == null)
        {
            throw new GamePlatformException(GameLibraries.WINE, "Wine was not found. Install Wine and Windows Steam/Subnautica in a Wine prefix, or set NITROX_WINE_EXE to the wine executable path.");
        }

        ProcessStartInfo? steamStartInfo = CreateSteamStartInfoIfNeeded(pathToGameExe, wineExecutable);
        if (steamStartInfo != null)
        {
            using Process? steamProcess = Process.Start(steamStartInfo);
            await Task.Delay(GetSteamStartupDelayMs());
        }

        ProcessStartInfo startInfo = CreateStartInfo(pathToGameExe, launchArguments, wineExecutable, NitroxUser.LauncherPath);
        ProcessEx? process = ProcessEx.From(startInfo);
        if (process == null)
        {
            throw new GamePlatformException(GameLibraries.WINE, "Subnautica exited immediately after starting through Wine.");
        }

        return process;
    }

    internal static ProcessStartInfo? CreateSteamStartInfoIfNeeded(string pathToGameExe, string wineExecutable)
    {
        string? steamExecutable = FindSteamExecutableForGame(pathToGameExe);
        if (steamExecutable == null)
        {
            return null;
        }

        return CreateStartInfo(steamExecutable, SteamLaunchArguments, wineExecutable, NitroxUser.LauncherPath);
    }

    internal static ProcessStartInfo CreateStartInfo(string pathToGameExe, string launchArguments, string wineExecutable, string launcherPath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = wineExecutable,
            WorkingDirectory = Path.GetDirectoryName(pathToGameExe) ?? Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            CreateNoWindow = false
        };

#if NET
        startInfo.ArgumentList.Add(pathToGameExe);
        foreach (string argument in ParseArguments(launchArguments))
        {
            startInfo.ArgumentList.Add(argument);
        }
#else
        startInfo.Arguments = string.Join(" ", new[] { QuoteArgument(pathToGameExe) }.Concat(ParseArguments(launchArguments).Select(QuoteArgument)));
#endif

        startInfo.EnvironmentVariables[NitroxUser.LAUNCHER_PATH_ENV_KEY] = launcherPath;
        startInfo.EnvironmentVariables[DisableIpv6EnvKey] = Environment.GetEnvironmentVariable(DisableIpv6EnvKey) ?? "1";
        startInfo.EnvironmentVariables[ClientConnectTimeoutMsEnvKey] = Environment.GetEnvironmentVariable(ClientConnectTimeoutMsEnvKey) ?? "10000";
        startInfo.EnvironmentVariables[LiteNetLibManualModeEnvKey] = Environment.GetEnvironmentVariable(LiteNetLibManualModeEnvKey) ?? "0";
        startInfo.EnvironmentVariables[LiteNetLibCrcFallbackEnvKey] = Environment.GetEnvironmentVariable(LiteNetLibCrcFallbackEnvKey) ?? "1";
        startInfo.EnvironmentVariables[LiteNetLibDisableNativeSocketsEnvKey] = Environment.GetEnvironmentVariable(LiteNetLibDisableNativeSocketsEnvKey) ?? "1";

        string? inferredPrefix = InferWinePrefix(pathToGameExe);
        if (!string.IsNullOrWhiteSpace(inferredPrefix) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINEPREFIX")))
        {
            startInfo.EnvironmentVariables["WINEPREFIX"] = inferredPrefix;
        }

        return startInfo;
    }

    internal static string? FindSteamExecutableForGame(string pathToGameExe)
    {
        string[] pathParts = pathToGameExe.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        int steamAppsIndex = Array.FindIndex(pathParts, static part => part.Equals("steamapps", StringComparison.OrdinalIgnoreCase));
        if (steamAppsIndex <= 0)
        {
            return null;
        }

        string steamPath = Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar.ToString(), pathParts.Take(steamAppsIndex));
        string steamExecutable = Path.Combine(steamPath, "steam.exe");
        return File.Exists(steamExecutable) ? steamExecutable : null;
    }

    internal static string? InferWinePrefix(string windowsExePath)
    {
        string[] pathParts = windowsExePath.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        int driveCIndex = Array.FindIndex(pathParts, static part => part.Equals("drive_c", StringComparison.OrdinalIgnoreCase));
        if (driveCIndex <= 0)
        {
            return null;
        }

        string prefix = Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar.ToString(), pathParts.Take(driveCIndex));
        return Directory.Exists(prefix) ? prefix : null;
    }

    internal static string? FindWineExecutable()
    {
        string? configuredPath = Environment.GetEnvironmentVariable(WineExecutableOverrideEnvKey);
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        return FindExecutableOnPath("wine64") ??
               FindExecutableOnPath("wine") ??
               FindExecutableInCommonMacOSLocations();
    }

    private static string? FindExecutableInCommonMacOSLocations()
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] candidatePaths =
        [
            "/opt/homebrew/bin/wine",
            "/opt/homebrew/bin/wine64",
            "/usr/local/bin/wine",
            "/usr/local/bin/wine64",
            Path.Combine(homePath, "Applications", "Wine Stable.app", "Contents", "Resources", "wine", "bin", "wine"),
            Path.Combine(homePath, "Applications", "Wine Stable.app", "Contents", "Resources", "wine", "bin", "wine64"),
            "/Applications/Wine Stable.app/Contents/Resources/wine/bin/wine",
            "/Applications/Wine Stable.app/Contents/Resources/wine/bin/wine64"
        ];

        return candidatePaths.FirstOrDefault(File.Exists);
    }

    private static string? FindExecutableOnPath(string executableName)
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (string path in pathValue.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            string executablePath = Path.Combine(path, executableName);
            if (File.Exists(executablePath))
            {
                return executablePath;
            }
        }

        return null;
    }

    internal static string[] ParseArguments(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return [];
        }

        List<string> parsedArguments = [];
        StringBuilder currentArgument = new();
        bool insideQuotes = false;

        for (int i = 0; i < arguments.Length; i++)
        {
            char character = arguments[i];
            if (character == '\\' && i + 1 < arguments.Length && arguments[i + 1] == '"')
            {
                currentArgument.Append('"');
                i++;
                continue;
            }

            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !insideQuotes)
            {
                AddArgumentIfPresent();
                continue;
            }

            currentArgument.Append(character);
        }

        AddArgumentIfPresent();
        return [.. parsedArguments];

        void AddArgumentIfPresent()
        {
            if (currentArgument.Length == 0)
            {
                return;
            }

            parsedArguments.Add(currentArgument.ToString());
            currentArgument.Clear();
        }
    }

    private static int GetSteamStartupDelayMs()
    {
        string? configuredDelay = Environment.GetEnvironmentVariable(SteamStartupDelayMsEnvKey);
        if (int.TryParse(configuredDelay, out int delay) && delay >= 0)
        {
            return delay;
        }

        return DefaultSteamStartupDelayMs;
    }

    private static string QuoteArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
        {
            return "\"\"";
        }

        if (!argument.Any(char.IsWhiteSpace) && !argument.Contains('"'))
        {
            return argument;
        }

        return $"\"{argument.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }
}
