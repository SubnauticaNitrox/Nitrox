using System.IO;

namespace NitroxLauncher.Patching
{
    public class PlatformDetection
    {
        public static bool IsEpic(string subnauticaPath)
        {

            if (Directory.Exists(Path.Combine(subnauticaPath, ".egstore")))
            {
                return true;
            }
            return false;
        }

        public static bool IsSteam(string subnauticaPath)
        {
            if (File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "Plugins", "CSteamworks.dll")))
            {
                return true;
            }
            return false;
        }
    }
}
