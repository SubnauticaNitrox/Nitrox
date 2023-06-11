using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using NitroxModel.Discovery.Models;

namespace NitroxModel.Discovery.InstallationFinders;

public class SteamGameRegistryFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        string steamPath = GetSteamPath();

        if (string.IsNullOrEmpty(steamPath))
        {
            errors.Add("Steam isn't installed");
            return null;
        }

        string appsPath = Path.Combine(steamPath, "steamapps");
        if (File.Exists(Path.Combine(appsPath, $"appmanifest_{gameInfo.SteamAppId}.acf")))
        {
            return new()
            {
                Path = Path.Combine(appsPath, "common", gameInfo.Name),
                GameInfo = gameInfo,
                Origin = GameLibraries.STEAM
            };
        }

        string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), gameInfo.SteamAppId, gameInfo.Name);
        if (string.IsNullOrWhiteSpace(path))
        {
            errors?.Add($"It appears you don't have {gameInfo.Name} installed anywhere. The game files are needed to run the server.");
            return null;
        }

        return new()
        {
            Path = path,
            GameInfo = gameInfo,
            Origin = GameLibraries.STEAM
        };
    }

    private static string GetSteamPath()
    {
        string steamPath = null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            steamPath = Platforms.OS.Windows.Internal.RegistryEx.Read<string>(@"Software\\Valve\\Steam\SteamPath");

            if (string.IsNullOrWhiteSpace(steamPath))
            {
                string homePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Steam"
                );

                if (Directory.Exists(homePath))
                {
                    steamPath = homePath;
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homePath))
            {
                homePath = Environment.GetEnvironmentVariable("HOME");
            }

            // Steam should always be here (~/.steam/steam is a symlink to .local/share/Steam)
            if (!string.IsNullOrWhiteSpace(homePath))
            {
                steamPath = Path.Combine(homePath, ".local", "share", "Steam");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homePath))
            {
                homePath = Environment.GetEnvironmentVariable("HOME");
            }

            // Steam should always be here
            if (!string.IsNullOrWhiteSpace(homePath))
            {
                steamPath = Path.Combine(homePath, "Library", "Application Support", "Steam");
            }
        }

        return steamPath;
    }

    /// <summary>
    ///     Finds game install directory by iterating through all the steam game libraries configured, matching the given appid.
    /// </summary>
    private static string SearchAllInstallations(string libraryFolders, int appid, string gameName)
    {
        if (!File.Exists(libraryFolders))
        {
            return null;
        }

        StreamReader file = new(libraryFolders);
        char[] trimChars = { ' ', '\t' };
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
