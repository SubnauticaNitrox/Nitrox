using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class SteamGameRegistryFinder : IFindGameInstallation
    {
        public string FindGame(IList<string> errors = null)
        {
            string steamPath = RegistryEx.Read<string>(@"Software\\Valve\\Steam\SteamPath");
            if (string.IsNullOrEmpty(steamPath))
            {
                errors?.Add("It appears you don't have Steam installed.");
                return null;
            }

            string appsPath = Path.Combine(steamPath, "steamapps");
#if SUBNAUTICA
            int steamAppId = GameInfo.Subnautica.SteamAppId;
            string steamAppName = GameInfo.Subnautica.Name;
#elif BELOWZERO
            int steamAppId = GameInfo.SubnauticaBelowZero.SteamAppId;
            string steamAppName = GameInfo.SubnauticaBelowZero.Name;
#endif
            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{steamAppId}.acf")))
            {
                return Path.Combine(appsPath, "common", steamAppName);
            }

            string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), steamAppId, steamAppName);
            if (string.IsNullOrEmpty(path))
            {
                errors?.Add($"It appears you don't have {steamAppName} installed anywhere. The game files are needed to run the server.");
            }
            else
            {
                return path;
            }

            return null;
        }

        /// <summary>
        ///     Finds game install directory by iterating through all the steam game libraries configured and finding the appid
        ///     that matches <see cref="GameInfo.Subnautica.SteamAppId" />.
        /// </summary>
        private static string SearchAllInstallations(string libraryFolders, int appid, string gameName)
        {
            if (!File.Exists(libraryFolders))
            {
                return null;
            }

            StreamReader file = new(libraryFolders);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = Regex.Unescape(line.Trim().Trim('\t'));
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
                    return Path.Combine(value, "steamapps/common", gameName);
                }
            }

            return null;
        }
    }
}
