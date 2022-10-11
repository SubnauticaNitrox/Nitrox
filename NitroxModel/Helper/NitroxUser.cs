using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NitroxModel.Discovery;
using NitroxModel.Platforms.OS.Windows.Internal;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Helper
{
    public static class NitroxUser
    {
        public const string LAUNCHER_PATH_ENV_KEY = "NITROX_LAUNCHER_PATH";
        private const string PREFERRED_GAMEPATH_REGKEY = @"SOFTWARE\Nitrox\PreferredGamePath";
        private static string launcherPath;
        private static string gamePath;

        private static readonly IEnumerable<Func<string>> launcherPathDataSources = new List<Func<string>>
        {
            () => Environment.GetEnvironmentVariable(LAUNCHER_PATH_ENV_KEY),
            () =>
            {
                Assembly currentAsm = Assembly.GetEntryAssembly();
                if (currentAsm?.GetName().Name.Equals("NitroxLauncher") ?? false)
                {
                    return Path.GetDirectoryName(currentAsm.Location);
                }
                return null;
            }
        };

        /// <summary>
        ///     Tries to get the launcher path that was previously saved by other Nitrox code.
        /// </summary>
        public static string LauncherPath
        {
            get
            {
                if (launcherPath != null)
                {
                    return launcherPath;
                }

                foreach (Func<string> retriever in launcherPathDataSources)
                {
                    string path = retriever();
                    if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    {
                        return launcherPath = path;
                    }
                }

                return null;
            }
        }

        public static string AssetsPath => Path.Combine(LauncherPath, "AssetBundles");

        public static string PreferredGamePath
        {
            get => RegistryEx.Read<string>(PREFERRED_GAMEPATH_REGKEY);
            set => RegistryEx.Write(PREFERRED_GAMEPATH_REGKEY, value);
        }

        public static IGamePlatform GamePlatform { get; private set; }

        public static string GamePath
        {
            get
            {
                if (!string.IsNullOrEmpty(gamePath))
                {
                    return gamePath;
                }

                List<string> errors = new();
                string path = GameInstallationFinder.Instance.FindGame(errors);
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    return gamePath = path;
                }

                Log.Error($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                return string.Empty;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                if (!Directory.Exists(value))
                {
                    throw new ArgumentException("Given path is an invalid directory");
                }

                // Ensures the path looks alright (no mixed / and \ path separators)
                gamePath = Path.GetFullPath(value);
                GamePlatform = GamePlatforms.GetPlatformByGameDir(gamePath);
            }
        }
    }
}
