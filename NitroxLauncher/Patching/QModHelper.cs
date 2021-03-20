using System.IO;
using NitroxModel.Logger;

namespace NitroxLauncher.Patching
{
    public static class QModHelper
    {
        private static readonly string originalFileName = "winhttp.dll";
        private static readonly string renamedFileName = "winhttp_nitrox_stopped.dll";
        private static bool QModPatched;
        public static void RemoveQModEntryPoint(string subnauticaBasePath)
        {
            if (QModPatched || !IsQModInstalled(subnauticaBasePath))
            {
                return;
            }
            Log.Info("Attempting to remove QMod initialisation");
            RenameFile(subnauticaBasePath, originalFileName, renamedFileName);
        }

        public static void RestoreQModEntryPoint(string subnauticaBasePath)
        {
            if (!QModPatched || !IsQModInstalled(subnauticaBasePath))
            {
                return;
            }
            Log.Info("Attempting to restore QMod initialisation");
            RenameFile(subnauticaBasePath, renamedFileName, originalFileName);
        }

        private static void RenameFile(string subnauticaBasePath, string fileToRename, string newFileName)
        {
            string fileToRenamePath = Path.Combine(subnauticaBasePath, fileToRename);
            if (!File.Exists(fileToRenamePath))
            {
                Log.Error($"QMod entry cannot be found, please uninstall QMod");
                return;
            }

            QModPatched = !QModPatched;
            string newFilePath = Path.Combine(subnauticaBasePath, newFileName);
            File.Move(fileToRenamePath, newFilePath);
            Log.Info("Removing/Restoring QMod initialisation has been successful");
        }

        private static bool IsQModInstalled(string subnauticaBasePath)
        {
            string subnauticaQModsPath = Path.Combine(subnauticaBasePath, "QMods");
            return Directory.Exists(subnauticaQModsPath);
        }
    }
}
