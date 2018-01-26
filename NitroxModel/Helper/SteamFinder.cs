using Microsoft.Win32;
using NitroxModel.Logger;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NitroxModel.Helper
{
    public class SteamFinder
    {
        public static string FindSteamGamePath(int appid, string gameName)
        {
            if (ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") == null)
            {
                Log.Info("You either don't have steam installed or your registry variable isn't set.");
                return "";
            }

            string appsPath = (string)ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") + "/steamapps/";

            if (File.Exists(appsPath + $"appmanifest_{appid.ToString()}.acf"))
            {
                return appsPath + "common/" + gameName;
            }
            else
            {
                string path = SearchAllInstalations(appsPath + "libraryfolders.vdf", appid, gameName);
                if (path == "")
                {
                    Log.Info($"It appears you don't have {gameName} installed anywhere. The game files are needed to run the server.");
                }
                else
                {
                    return path;
                }
            }
            return "";
        }

        private static string SearchAllInstalations(string libraryfolders, int appid, string gameName)
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
                    if (File.Exists(value + $"/steamapps/appmanifest_{appid.ToString()}.acf"))
                    {
                        return value + "/steamapps/common/" + gameName;
                    }
                }
            }
            return "";
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
