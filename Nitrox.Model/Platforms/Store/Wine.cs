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
    public string Name => nameof(Wine);
    public Platform Platform => Platform.WINE;

    public bool OwnsGame(string gameDirectory)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
               GameInstallationHelper.IsWindowsGameLayout(gameDirectory, GameInfo.Subnautica);
    }

    public static Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        string? wineExecutable = FindWineExecutable();
        if (wineExecutable == null)
        {
            throw new GamePlatformException(GameLibraries.WINE, "Wine was not found. Install Wine and Windows Steam/Subnautica in a Wine prefix, or set NITROX_WINE_EXE to the wine executable path.");
        }

        ProcessStartInfo startInfo = CreateStartInfo(pathToGameExe, launchArguments, wineExecutable, NitroxUser.LauncherPath);
        ProcessEx? process = ProcessEx.From(startInfo);
        if (process == null)
        {
            throw new GamePlatformException(GameLibraries.WINE, "Subnautica exited immediately after starting through Wine.");
        }

        return Task.FromResult(process);
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
        string? inferredPrefix = InferWinePrefix(pathToGameExe);
        if (!string.IsNullOrWhiteSpace(inferredPrefix) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINEPREFIX")))
        {
            startInfo.EnvironmentVariables["WINEPREFIX"] = inferredPrefix;
        }

        return startInfo;
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
