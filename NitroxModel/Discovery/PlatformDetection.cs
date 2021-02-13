using System.IO;

namespace NitroxModel.Discovery
{
    public class PlatformDetection
    {

        public static Platform GetPlatform(string subnauticaPath)
        {
            if (Directory.Exists(Path.Combine(subnauticaPath, ".egstore")))
            {
                if (Directory.GetFiles(Path.Combine(subnauticaPath, ".egstore")).Length > 0)
                {
                    return Platform.EPIC;
                }
            }

#if SUBNAUTICA
            if (File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "Plugins", "CSteamworks.dll")))
#elif BELOWZERO
            if (File.Exists(Path.Combine(subnauticaPath, "SubnauticaZero_Data", "Plugins", "x86_64", "CSteamworks.dll")))
#endif
            {
                return Platform.STEAM;
            }

            if (File.Exists(Path.Combine(subnauticaPath, "appxmanifest.xml")))
            {
                return Platform.MICROSOFT;
            }

            return Platform.NONE;
        }
    }
}
