using System.IO;
using System.Runtime.InteropServices;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Extensions;

public static class ServerStartOptionsExtensions
{
    private static string subnauticaResourceAssetsPath;

    public static string GetSubnauticaResourcesPath(this ServerStartOptions startOptions)
    {
        if (subnauticaResourceAssetsPath != null)
        {
            return subnauticaResourceAssetsPath;
        }

        return subnauticaResourceAssetsPath = FindAssetsPath(startOptions);

        static string FindAssetsPath(ServerStartOptions startOptions)
        {
            if (string.IsNullOrEmpty(startOptions.GameInstallPath))
            {
                throw new DirectoryNotFoundException($"Could not locate {GameInfo.Subnautica.FullName} installation directory.");
            }

            if (File.Exists(Path.Combine(startOptions.GameInstallPath, GameInfo.Subnautica.DataFolder, "resources.assets")))
            {
                return Path.Combine(startOptions.GameInstallPath, GameInfo.Subnautica.DataFolder);
            }
            if (File.Exists(Path.Combine("..", "resources.assets"))) //  SubServer => Subnautica/Subnautica_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine(".."));
            }
            if (File.Exists(Path.Combine("..", GameInfo.Subnautica.DataFolder, "resources.assets"))) //  SubServer => Subnautica/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", GameInfo.Subnautica.DataFolder));
            }
            if (File.Exists("resources.assets")) //  SubServer/* => Subnautica/Subnautica_Data/
            {
                return Directory.GetCurrentDirectory();
            }
            throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
        }
    }

    public static string GetSubnauticaAaResourcePath(this ServerStartOptions startOptions) => Path.Combine(startOptions.GetSubnauticaResourcesPath(), "StreamingAssets", "aa");

    public static string GetSubnauticaManagedPath(this ServerStartOptions startOptions) => Path.Combine(startOptions.GetSubnauticaResourcesPath(), "Managed");

    public static string GetSubnauticaStandaloneResourcePath(this ServerStartOptions startOptions)
    {
        string standaloneFolderName = "StandaloneWindows64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            standaloneFolderName = "StandaloneOSX";
        }
        return Path.Combine(startOptions.GetSubnauticaAaResourcePath(), standaloneFolderName);
    }

    public static string GetSubnauticaBuild18Path(this ServerStartOptions startOptions) => Path.Combine(startOptions.GetSubnauticaResourcesPath(), "StreamingAssets", "SNUnmanagedData", "Build18");

    public static string GetServerConfigFilePath(this ServerStartOptions options) => Path.Combine(options.GetServerSavePath(), "server.cfg");

    public static string GetServerSavePath(this ServerStartOptions options) => Path.Combine(KeyValueStore.Instance.GetServerSavesPath(), options.SaveName);

    public static string GetServerCachePath(this ServerStartOptions options) => Path.Combine(options.NitroxAppDataPath, "Cache");

    public static string GetServerLogsPath(this ServerStartOptions options) => Path.Combine(options.NitroxAppDataPath, "Logs");
}
