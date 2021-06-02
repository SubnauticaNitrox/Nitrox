using System;
using System.IO;
using NitroxModel.Logger;

namespace NitroxLauncher.Patching
{
    public static class QModHelper
    {
        private static string qmmPluginOriginPath;
        private static string qmmPatchersOriginPath;
        private static string qmmPluginBackupPath;
        private static string qmmPatchersBackupPath;
        private static bool qModPatched = true;

        public static void RemoveQModManagerFolders(string subnauticaBasePath)
        {
            if (!qModPatched || !IsQModInstalled(subnauticaBasePath))
            {
                return;
            }

            string pluginOriginPath = Path.Combine(subnauticaBasePath, "BepInEx", "plugins", "QModManager");
            string patchersOriginPath = Path.Combine(subnauticaBasePath, "BepInEx", "patchers", "QModManager");
            if (!Directory.Exists(pluginOriginPath) || !Directory.Exists(patchersOriginPath))
            {
                return;
            }
            qmmPluginOriginPath = pluginOriginPath;
            qmmPatchersOriginPath = patchersOriginPath;
            qmmPluginBackupPath = Path.Combine(subnauticaBasePath, "QMMBackup", "plugins");
            qmmPatchersBackupPath = Path.Combine(subnauticaBasePath, "QMMBackup", "patchers");
            Directory.CreateDirectory(qmmPluginBackupPath);
            Directory.CreateDirectory(qmmPatchersBackupPath);


            Log.Info("Attempting to remove QModManager");

            Exception error = NitroxEntryPatch.RetryWait(() => Directory.Move(qmmPatchersOriginPath, Path.Combine(qmmPatchersBackupPath, "QModManager")), 100, 5) ?? NitroxEntryPatch.RetryWait(() => Directory.Move(qmmPluginOriginPath, Path.Combine(qmmPluginBackupPath, "QModManager")), 100, 5);

            if (error != null)
            {
                Log.Error(error, "Unable to remove QModManager.");
                throw error;
            }
            Log.Info("Successfully removed QModManager");
            qModPatched = false;
        }

        public static void RestoreQModManagerFolders(string subnauticaBasePath)
        {
            if (qModPatched || IsQModInstalled(subnauticaBasePath))
            {
                return;
            }

            string pluginBackupPath = Path.Combine(subnauticaBasePath, "QMMBackup", "plugins", "QModManager");
            string patchersBackupPath = Path.Combine(subnauticaBasePath, "QMMBackup", "patchers", "QModManager");
            if (!Directory.Exists(pluginBackupPath) || !Directory.Exists(patchersBackupPath))
            {
                return;
            }
            qmmPluginOriginPath = Path.Combine(subnauticaBasePath, "BepInEx", "plugins");
            qmmPatchersOriginPath = Path.Combine(subnauticaBasePath, "BepInEx", "patchers");
            qmmPluginBackupPath = pluginBackupPath;
            qmmPatchersBackupPath = patchersBackupPath;

            Log.Info("Attempting to restore QModManager");
            Exception error = NitroxEntryPatch.RetryWait(() => Directory.Move(qmmPatchersBackupPath, Path.Combine(qmmPatchersOriginPath, "QModManager")), 100, 5) ?? NitroxEntryPatch.RetryWait(() => Directory.Move(qmmPluginBackupPath, Path.Combine(qmmPluginOriginPath, "QModManager")), 100, 5);
            if (error != null)
            {
                Log.Error(error, "Unable to restore QModManager.");
                throw error;
            }

            Directory.Delete(Path.Combine(subnauticaBasePath, "QMMBackup"), true);

            Log.Info("Successfully restored QModManager");
            qModPatched = true;
        }
        
        private static bool IsQModInstalled(string subnauticaBasePath)
        {
            string subnauticaQModsPath = Path.Combine(subnauticaBasePath, "BepInEx", "plugins", "QModManager", "QModInstaller.dll");
            return File.Exists(subnauticaQModsPath);
        }
    }
}
