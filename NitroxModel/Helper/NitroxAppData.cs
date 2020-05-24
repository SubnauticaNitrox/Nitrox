using System;
using System.IO;

namespace NitroxModel.Helper
{
    public class NitroxAppData
    {
        private static NitroxAppData instance;

        private NitroxAppData()
        {
            Directory.CreateDirectory(RootDir);
        }

        public static NitroxAppData Instance => instance ?? (instance = new NitroxAppData());

        private string RootDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox");

        /// <summary>
        ///     Gets the launcher path from the environment variable.
        ///     Set by Nitrox.Bootloader.Main.
        /// </summary>
        public string LauncherPath => Environment.GetEnvironmentVariable("NITROX_LAUNCHER_PATH");

        public string AssetsPath => Path.Combine(LauncherPath, "AssetBundles");

        public static NitroxAppData Load()
        {
            return new NitroxAppData();
        }
    }
}
