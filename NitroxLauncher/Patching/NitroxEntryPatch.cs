using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Logger;

namespace NitroxLauncher.Patching
{
    internal class NitroxEntryPatch
    {
        public const string BEPINEX_ASSEMBLY_NAME = "BepInEx.dll";
        public const string NITROX_ASSEMBLY_NAME = "Nitrox.Bootloader.dll";
        public const string QMODMANAGER_ASSEMBLY_NAME = "QModInstaller.dll";
        
        private readonly string bepinexCorePath;
        private readonly string bepinexPluginsPath;
        private readonly string subnauticaCorePath;

        public bool IsApplied => IsPatchApplied();

        public NitroxEntryPatch(string subnauticaBasePath)
        {
            subnauticaCorePath = subnauticaBasePath;
            bepinexCorePath = Path.Combine(subnauticaBasePath, "BepInEx", "Core");
            bepinexPluginsPath = Path.Combine(subnauticaBasePath, "BepInEx", "plugins");
        }

        public void Apply()
        {
            string nitroxLibPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lib");
            string bepinex = Path.Combine(bepinexCorePath, BEPINEX_ASSEMBLY_NAME);
            string bepinexSourcePath = Path.Combine(nitroxLibPath, "BepInEx");
            string bepinexInstallPath = subnauticaCorePath;
            string nitroxFolder = Path.Combine(bepinexPluginsPath, "Nitrox");
            string nitroxBootloaderDestination = Path.Combine(nitroxFolder, NITROX_ASSEMBLY_NAME);
            string nitroxBootloaderSource = Path.Combine(nitroxLibPath, NITROX_ASSEMBLY_NAME);
            
            Exception error;

            if (!File.Exists(bepinex))
            {
                error = RetryWait(() => RecursiveCopyBepInExFolder(bepinexSourcePath, bepinexInstallPath), 100, 5);
                if (error != null)
                {
                    Log.Error(error, "Unable to install BepInEx.");
                    throw error;
                }
            }

            QModHelper.RemoveQModManagerFolders(subnauticaCorePath);

            if (File.Exists(nitroxBootloaderDestination))
            {
                error = RetryWait(() => File.Delete(nitroxBootloaderDestination), 100, 5);
                if (error != null)
                {
                    Log.Error(error, "Unable to delete bootloader dll.");
                    throw error;
                }
            }
            
            error = RetryWait(() => Directory.CreateDirectory(nitroxFolder), 100, 5);
            if (error != null)
            {
                Log.Error(error, "Unable to create Nitrox folder.");
                throw error;
            }

            error = RetryWait(() => File.Copy(nitroxBootloaderSource, nitroxBootloaderDestination), 100, 5);
            if (error != null)
            {
                Log.Error(error, "Unable to move bootloader dll.");
                throw error;
            }
        }

        private static void RecursiveCopyBepInExFolder(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);
            
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }
            
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                RecursiveCopyBepInExFolder(subdir.FullName, tempPath);
            }
        }

        internal static Exception RetryWait(Action action, int interval, int retries = 0)
        {
            Exception lastException = null;
            while (retries >= 0)
            {
                try
                {
                    retries--;
                    action();
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Task.Delay(interval).Wait();
                }
            }
            return lastException;
        }
        
        public void Remove()
        {
            string nitroxFolder = Path.Combine(bepinexPluginsPath, "Nitrox");
            string nitroxBootloaderDestination = Path.Combine(nitroxFolder, NITROX_ASSEMBLY_NAME);

            if (File.Exists(nitroxBootloaderDestination))
            {
                Exception error = RetryWait(() => File.Delete(nitroxBootloaderDestination), 100, 5);
                if (error != null)
                {
                    Log.Error(error, "Unable to delete bootloader dll.");
                    throw error;
                }
                error = RetryWait(() => Directory.Delete(nitroxFolder), 100, 5);
                if (error != null)
                {
                    Log.Error(error, "Unable to delete nitrox plugin folder.");
                    throw error;
                }
                
                QModHelper.RestoreQModManagerFolders(subnauticaCorePath);
            }


        }

        private bool IsPatchApplied()
        {
            string bepinex = Path.Combine(bepinexCorePath, BEPINEX_ASSEMBLY_NAME);
            string nitroxBootloaderDestination = Path.Combine(bepinexPluginsPath, "Nitrox", NITROX_ASSEMBLY_NAME);

            return File.Exists(bepinex) && File.Exists(nitroxBootloaderDestination);
        }
    }
}
