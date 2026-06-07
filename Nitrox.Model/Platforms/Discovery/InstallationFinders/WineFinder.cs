using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using static Nitrox.Model.Platforms.Discovery.InstallationFinders.Core.GameFinderResult;

namespace Nitrox.Model.Platforms.Discovery.InstallationFinders;

/// <summary>
///     Finds Windows Subnautica installs inside common macOS Wine-style prefixes.
/// </summary>
public sealed class WineFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Error("Wine prefix detection is only supported on macOS");
        }

        foreach (string prefixPath in GetCandidatePrefixes())
        {
            foreach (string steamPath in GetSteamPaths(prefixPath))
            {
                string appsPath = Path.Combine(steamPath, "steamapps");
                string? path = null;
                if (File.Exists(Path.Combine(appsPath, $"appmanifest_{gameInfo.SteamAppId}.acf")))
                {
                    path = Path.Combine(appsPath, "common", gameInfo.Name);
                }
                else
                {
                    path = SteamFinder.SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), gameInfo.SteamAppId, gameInfo.Name);
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                path = GameInstallationHelper.NormalizeGamePath(path, gameInfo);
                if (GameInstallationHelper.IsWindowsGameLayout(path, gameInfo))
                {
                    return Ok(path);
                }
            }
        }

        return NotFound();
    }

    internal static IEnumerable<string> GetCandidatePrefixes(string? homePath = null)
    {
        homePath ??= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(homePath))
        {
            homePath = Environment.GetEnvironmentVariable("HOME");
        }
        if (string.IsNullOrWhiteSpace(homePath) || !Directory.Exists(homePath))
        {
            yield break;
        }

        foreach (string path in EnumerateExistingDirectories(Path.Combine(homePath, ".wine")))
        {
            yield return path;
        }

        foreach (string path in EnumerateExistingDirectories(Path.Combine(homePath, ".local", "share", "wineprefixes"), "*"))
        {
            yield return path;
        }

        foreach (string path in EnumerateExistingDirectories(Path.Combine(homePath, "Games"), "*"))
        {
            if (Directory.Exists(Path.Combine(path, "drive_c")))
            {
                yield return path;
            }
        }

        foreach (string path in EnumerateExistingDirectories(Path.Combine(homePath, "Library", "Application Support", "CrossOver", "Bottles"), "*"))
        {
            yield return path;
        }
    }

    private static IEnumerable<string> GetSteamPaths(string prefixPath)
    {
        string driveC = Path.Combine(prefixPath, "drive_c");
        if (!Directory.Exists(driveC))
        {
            yield break;
        }

        string[] candidatePaths =
        [
            Path.Combine(driveC, "Program Files (x86)", "Steam"),
            Path.Combine(driveC, "Program Files", "Steam")
        ];

        foreach (string path in candidatePaths.Where(Directory.Exists))
        {
            yield return path;
        }
    }

    private static IEnumerable<string> EnumerateExistingDirectories(string path, string? searchPattern = null)
    {
        if (searchPattern == null)
        {
            if (Directory.Exists(path))
            {
                yield return path;
            }
            yield break;
        }

        if (!Directory.Exists(path))
        {
            yield break;
        }

        foreach (string directory in Directory.EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly))
        {
            yield return directory;
        }
    }
}
