using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Store;
using static Nitrox.Model.Platforms.Discovery.InstallationFinders.Core.GameFinderResult;

namespace Nitrox.Model.Platforms.Discovery.InstallationFinders;

/// <summary>
/// Trying to find the path in the Steam installation directory by the appid that contains the game installation directory.
/// By default each game will have a corresponding appmanifest_{appid}.acf file in the steamapps folder.
/// Except for some games that are installed on a different diskdrive, in those case 'libraryfolders.vdf' will give us the real location of the appid folder.
/// </summary>
public sealed class SteamFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string steamPath = GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            return Error("Steam isn't installed");
        }

        string path;
        string appsPath = Path.Combine(steamPath, "steamapps");
        if (File.Exists(Path.Combine(appsPath, $"appmanifest_{gameInfo.SteamAppId}.acf")))
        {
            path = Path.Combine(appsPath, "common", gameInfo.Name);
        }
        else
        {
            path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), gameInfo.SteamAppId, gameInfo.Name);
            if (string.IsNullOrWhiteSpace(path))
            {
                return NotFound();
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            path = Path.Combine(path, $"{gameInfo.Name}.app", "Contents");
        }
        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            return Error($"Path '{path}' known by Steam for '{gameInfo.FullName}' does not point to a valid game file structure");
        }

        return Ok(path);
    }

    private static string GetSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homePath))
            {
                homePath = Environment.GetEnvironmentVariable("HOME");
            }
            if (!Directory.Exists(homePath))
            {
                return "";
            }

            // Steam should always be here
            string steamPath = Path.Combine(homePath, "Library", "Application Support", "Steam");
            if (Directory.Exists(steamPath))
            {
                return steamPath;
            }
        }

        return Path.GetDirectoryName(Steam.GetExeFile()) ?? "";
    }

    /// <summary>
    /// Finds game install directory by iterating through all the steam game libraries configured, matching the given appid.
    /// </summary>
    private static string? SearchAllInstallations(string libraryFolders, int appid, string gameName)
    {
        if (!File.Exists(libraryFolders))
        {
            return null;
        }

        StreamReader file = new(libraryFolders);
        char[] trimChars = [' ', '\t'];

        while (file.ReadLine() is { } line)
        {
            line = Regex.Unescape(line.Trim(trimChars));
            Match regMatch = Regex.Match(line, "\"(.*)\"\t*\"(.*)\"");
            string key = regMatch.Groups[1].Value;

            // New format (about 2021-07-16) uses "path" key instead of steam-library-index as key. If either, it could be steam game path.
            if (!key.Equals("path", StringComparison.OrdinalIgnoreCase) && !int.TryParse(key, out _))
            {
                continue;
            }

            string value = regMatch.Groups[2].Value;

            if (File.Exists(Path.Combine(value, "steamapps", $"appmanifest_{appid}.acf")))
            {
                return Path.Combine(value, "steamapps", "common", gameName);
            }
        }

        return null;
    }
}
