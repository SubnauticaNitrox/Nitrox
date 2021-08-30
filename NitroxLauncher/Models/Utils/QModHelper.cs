using System;
using System.IO;
using NitroxModel.Logger;
using NitroxModel.OS;

namespace NitroxLauncher.Models.Utils
{
    public static class QModHelper
    {
        private const string RENAMED_FILE_NAME = "winhttp_nitrox_stopped.dll";
        private const string ORIGINAL_FILE_NAME = "winhttp.dll";
        private static bool qModPatched;

        public static void RemoveQModEntryPoint(string subnauticaBasePath)
        {
            if (qModPatched || !IsQModInstalled(subnauticaBasePath))
            {
                return;
            }
            Log.Info("Attempting to remove QMod initialisation");
            RenameFile(subnauticaBasePath, ORIGINAL_FILE_NAME, RENAMED_FILE_NAME);
        }

        public static void RestoreQModEntryPoint(string subnauticaBasePath)
        {
            if (!qModPatched || !IsQModInstalled(subnauticaBasePath))
            {
                return;
            }
            Log.Info("Attempting to restore QMod initialisation");
            RenameFile(subnauticaBasePath, RENAMED_FILE_NAME, ORIGINAL_FILE_NAME);
        }

        private static void RenameFile(string subnauticaBasePath, string fileToRename, string newFileName)
        {
            string fileToRenamePath = Path.Combine(subnauticaBasePath, fileToRename);
            if (!File.Exists(fileToRenamePath))
            {
                Log.Error("QMod entry cannot be found, please uninstall QMod");
                return;
            }

            try
            {
                string newFilePath = Path.Combine(subnauticaBasePath, newFileName);
                FileSystem.Instance.ReplaceFile(fileToRenamePath, newFilePath);
                qModPatched = !qModPatched;
                Log.Info("Removing/Restoring QMod initialisation has been successful");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QMod entry cannot be removed/restored, please uninstall QMod");
            }
        }

        private static bool IsQModInstalled(string subnauticaBasePath)
        {
            string subnauticaQModsPath = Path.Combine(subnauticaBasePath, "QMods");
            return Directory.Exists(subnauticaQModsPath);
        }
    }
}
