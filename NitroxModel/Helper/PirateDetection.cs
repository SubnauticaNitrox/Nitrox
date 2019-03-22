using System.IO;

namespace NitroxModel.Helper
{
    public class PirateDetection
    {
        public static bool IsPirate(string subnauticaRoot)
        {
            string steamDll = Path.Combine(subnauticaRoot, "steam_api64.dll");

            // Check for a modified steam dll
            if (File.Exists(steamDll))
            {
                FileInfo fileInfo = new System.IO.FileInfo(steamDll);

                if (fileInfo.Length > 209000)
                {
                    return true;
                }
            }

            // Check for ini files in the root
            FileInfo[] iniFiles = new DirectoryInfo(subnauticaRoot).GetFiles("*.ini");

            if (iniFiles.Length > 0)
            {
                return true;
            }

            return false;
        }
    }
}
