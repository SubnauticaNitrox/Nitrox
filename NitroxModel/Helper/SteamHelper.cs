using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxModel.Helper
{
    public static class SteamHelper
    {
        public static readonly int SUBNAUTICA_APP_ID = 264710;
        public static readonly string SUBNAUTICA_GAME_NAME = "Subnautica";

        public static Optional<string> FindSubnauticaPath()
        {
            return FindSteamGamePath(SUBNAUTICA_APP_ID, SUBNAUTICA_GAME_NAME);
        }

        public static Optional<string> FindSteamGamePath(int appid, string gameName)
        {
            if (ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") == null)
            {
                Log.Info("You either don't have steam installed or your registry variable isn't set. Attempting to use \".\\path.txt\" fallback.");
                string tmpPath = Path.Combine(Path.GetFullPath("."), "path.txt");
                if (File.Exists(tmpPath))
                {
                    tmpPath = Path.Combine(tmpPath, "path.txt");
                    StreamReader stream = new StreamReader(File.OpenRead(tmpPath));
                    Optional<string> tmpString = Optional<string>.Of(stream.ReadLine());
                    if (tmpString.IsEmpty())
                    {
                        Log.Info("File is empty creating a new one");
                        tmpString = Optional<string>.Of(Path.GetFullPath("."));

                        StreamWriter tmpStream = new StreamWriter(File.OpenWrite(tmpString.Get()), System.Text.Encoding.UTF8);
                        tmpStream.WriteLine(Path.GetFullPath("."));
                        tmpStream.Close();
                        return tmpString;
                    }

                    return tmpString;
                }
            }

            string appsPath = Path.Combine((string)ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath"), "steamapps");

            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{appid}.acf")))
            {
                return Optional<string>.Of(Path.Combine(Path.Combine(appsPath, "common"), gameName));
            }

            if (File.Exists(Path.Combine(Path.GetFullPath("."), "path.txt")))
            {
                string tmpPath = Path.Combine(Path.GetFullPath("."), "path.txt");
                StreamReader stream = new StreamReader(File.OpenRead(tmpPath));
                Optional<string> tmpString = Optional<string>.Of(stream.ReadLine());
                if (tmpString.IsEmpty())
                {
                    Log.Info("File is empty!");
                    tmpString = Optional<string>.Of(Path.GetFullPath("."));

                    StreamWriter tmpStream = new StreamWriter(File.OpenWrite(Path.Combine(tmpString.Get(), "path.txt")), System.Text.Encoding.UTF8);
                    tmpStream.WriteLine(Path.GetFullPath("."));
                    tmpStream.Close();
                    return tmpString;
                }

                return tmpString;
            }
            
            string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), appid, gameName);
            if (path == null)
            {
                Log.Info($"It appears you don't have {gameName} installed anywhere. The game files are needed to run the server.");
            }
            else
            {
                return Optional<string>.Of(path);
            }

            StreamWriter streamW = new StreamWriter(File.OpenWrite(Path.Combine(Path.GetFullPath("."), "path.txt")), System.Text.Encoding.UTF8);
            streamW.WriteLine(Path.GetFullPath("."));
            streamW.Close();

            return Optional<string>.Of(Path.GetFullPath("."));
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
