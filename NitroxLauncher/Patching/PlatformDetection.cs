using System.IO;

namespace NitroxLauncher.Patching
{
    public static class PlatformDetection
    {
        public static bool IsEpic(string subnauticaPath)
        {
            return Directory.Exists(Path.Combine(subnauticaPath, ".egstore"));
        }

        public static bool IsSteam(string subnauticaPath)
        {
            return File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "Plugins", "CSteamworks.dll"));
        }
        public static bool IsMicrosoftStore(string subnauticaPath)
        {
            if (File.Exists(Path.Combine(subnauticaPath, "appxmanifest.xml")))
            {
                return true;
            }
            return false;
        }
    }
}
