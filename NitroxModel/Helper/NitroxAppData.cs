using System;
using System.IO;

namespace NitroxModel.Helper
{
    public class NitroxAppData
    {
        private static NitroxAppData instance;
        private string launcherPath;

        private NitroxAppData()
        {
            Directory.CreateDirectory(RootDir);
        }

        public static NitroxAppData Instance => instance ?? (instance = new NitroxAppData());

        private string RootDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox");

        public string LauncherPath
        {
            get
            {
                if (launcherPath != null && Directory.Exists(launcherPath))
                {
                    return launcherPath;
                }

                string file = Path.Combine(RootDir, "launcherpath.txt");
                if (!File.Exists(file))
                {
                    return null;
                }

                try
                {
                    string value = File.ReadAllText(file).Trim();
                    if (!Directory.Exists(value))
                    {
                        return null;
                    }
                    return launcherPath = value;
                }
                catch
                {
                    // ignored
                }
                return null;
            }
        }

        public string AssetsPath => Path.Combine(LauncherPath, "AssetBundles");

        public static NitroxAppData Load()
        {
            return new NitroxAppData();
        }
    }
}
