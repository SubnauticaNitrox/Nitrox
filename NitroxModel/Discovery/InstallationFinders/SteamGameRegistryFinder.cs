using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class SteamGameRegistryFinder : IFindGameInstallation
    {
        public string FindGame(IList<string> errors = null)
        {
            string steamPath = "";
            //TODO: handle other OSes
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // steam is ALWAYS installed to the user's folder on linux
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrWhiteSpace(homePath))
                {
                    steamPath = Path.Combine(homePath, ".local", "share", "Steam");
                }
                else
                {
                    errors?.Add("User HOME is not defined.");
                }
            }
            else
            {
                steamPath = RegistryEx.Read<string>(@"Software\\Valve\\Steam\SteamPath");
            }
            if (string.IsNullOrEmpty(steamPath))
            {
                errors?.Add("It appears you don't have Steam installed.");
                return null;
            }
            string appsPath = Path.Combine(steamPath, "steamapps");
            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{GameInfo.Subnautica.SteamAppId}.acf")))
            {
                return Path.Combine(appsPath, "common", GameInfo.Subnautica.Name);
            }
            string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), GameInfo.Subnautica.SteamAppId, GameInfo.Subnautica.Name);
            if (string.IsNullOrEmpty(path))
            {
                errors?.Add($"It appears you don't have {GameInfo.Subnautica.Name} installed anywhere. The game files are needed to run the server.");
            }
            else
            {
                return path;
            }

            return null;
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
}
