using System.IO;

namespace NitroxModel.Discovery
{
    public class PlatformDetection
    {

        public static Platform GetPlatform(string subnauticaPath)
        {
            if (Directory.Exists(Path.Combine(subnauticaPath, ".egstore")))
            {
                return Platform.EPIC;
            }

            if (File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "Plugins", "CSteamworks.dll")))
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
