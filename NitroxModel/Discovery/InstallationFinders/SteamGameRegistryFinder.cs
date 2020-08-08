using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class SteamGameRegistryFinder : IFindGameInstallation
    {
        private const int SUBNAUTICA_APP_ID = 264710;
        private const string SUBNAUTICA_GAME_NAME = "Subnautica";

        public string FindGame(List<string> errors = null)
        {
            string steamPath = (string)ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath");
            if (string.IsNullOrEmpty(steamPath))
            {
                errors?.Add("It appears you don't have Steam installed.");
                return null;
            }

            string appsPath = Path.Combine(steamPath, "steamapps");
            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{SUBNAUTICA_APP_ID}.acf")))
            {
                return Path.Combine(appsPath, "common", SUBNAUTICA_GAME_NAME);
            }

            string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), SUBNAUTICA_APP_ID, SUBNAUTICA_GAME_NAME);
            if (string.IsNullOrEmpty(path))
            {
                errors?.Add($"It appears you don't have {SUBNAUTICA_GAME_NAME} installed anywhere. The game files are needed to run the server.");
            }
            else
            {
                return path;
            }

            return null;
        }

        /// <summary>
        ///     Finds game install directory by iterating through all the steam game libraries configured and finding the appid
        ///     that matches <see cref="SUBNAUTICA_APP_ID" />.
        /// </summary>
        /// <param name="libFolders"></param>
        /// <param name="appid"></param>
        /// <param name="gameName"></param>
        /// <returns></returns>
        private static string SearchAllInstallations(string libFolders, int appid, string gameName)
        {
            if (!File.Exists(libFolders))
            {
                return null;
            }

            using (StreamReader file = new StreamReader(libFolders))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    line = Regex.Unescape(line.Trim().Trim('\t'));
                    Match regMatch = Regex.Match(line, "\"(.*)\"\t*\"(.*)\"");
                    string key = regMatch.Groups[1].Value;
                    string value = regMatch.Groups[2].Value;
                    if (int.TryParse(key, out int _) && File.Exists(Path.Combine(value, $"steamapps/appmanifest_{appid}.acf")))
                    {
                        return Path.Combine(value, "steamapps/common", gameName);
                    }
                }
            }

            return null;
        }

        private static object ReadRegistrySafe(string path, string key)
        {
            using (RegistryKey subKey = Registry.CurrentUser.OpenSubKey(path))
            {
                if (subKey != null)
                {
                    return subKey.GetValue(key);
                }
            }

            return null;
        }
    }
}
