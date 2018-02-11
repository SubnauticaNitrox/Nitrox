using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxModel.Helper
{
    public static class SteamFinder
    {
        public static Optional<string> FindSteamGamePath(int appid, string gameName)
        {
            object steamPathRegistry = ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath");
            if (steamPathRegistry == null)
            {
                Log.Info("You either don't have steam installed or your registry variable isn't set.");
                return Optional<string>.Empty();
            }

            string steamAppsPath = Path.Combine((string)steamPathRegistry, "steamapps");

            if (File.Exists(Path.Combine(steamAppsPath, $"appmanifest_{appid}.acf")))
            {
                return Optional<string>.Of(Path.Combine(Path.Combine(steamAppsPath, "common"), gameName));
            }

            string path = SearchAllInstallations(Path.Combine(steamAppsPath, "libraryfolders.vdf"), appid, gameName);
            if (path == null)
            {
                Log.Info($"It appears you don't have {gameName} installed anywhere. The game files are needed to run the server.");
                return Optional<string>.Empty();
            }

            return Optional<string>.Of(path);
        }

        private static string SearchAllInstallations(string libraryfolders, int appid, string gameName)
        {
            StreamReader file = new StreamReader(libraryfolders);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line.Trim();
                line.Trim('\t');
                line = Regex.Unescape(line);
                Match regMatch = Regex.Match(line, "\"(.*)\"\t*\"(.*)\"");
                string key = regMatch.Groups[1].Value;
                string value = regMatch.Groups[2].Value;
                int number;
                if (int.TryParse(key, out number))
                {
                    if (File.Exists(Path.Combine(value, $"steamapps/appmanifest_{appid}.acf")))
                    {
                        return Path.Combine(Path.Combine(value, "steamapps/common"), gameName);
                    }
                }
            }
            return null;
        }

        private static object ReadRegistrySafe(string path, string key)
        {
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path))
            {
                if (subkey != null)
                {
                    return subkey.GetValue(key);
                }
            }
            return null;
        }
    }
}
