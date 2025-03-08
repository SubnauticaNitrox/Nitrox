using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NitroxModel.Discovery;
using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Helper;

public static class NitroxUser
{
    public const string LAUNCHER_PATH_ENV_KEY = "NITROX_LAUNCHER_PATH";
    private const string PREFERRED_GAMEPATH_KEY = "PreferredGamePath";
    private static string appDataPath;
    private static string launcherPath;
    private static string gamePath;
    private static string executableRootPath;
    private static string executablePath;
    private static string assetsPath;

    private static readonly IEnumerable<Func<string>> launcherPathDataSources = new List<Func<string>>
    {
        () => Environment.GetEnvironmentVariable(LAUNCHER_PATH_ENV_KEY),
        () =>
        {
            Assembly currentAsm = Assembly.GetEntryAssembly();
            if (currentAsm?.GetName().Name.Equals("Nitrox.Launcher") ?? false)
            {
                return Path.GetDirectoryName(currentAsm.Location);
            }

            Assembly execAsm = Assembly.GetExecutingAssembly();
            string execDir = string.IsNullOrEmpty(execAsm.Location) ? Directory.GetCurrentDirectory() : execAsm.Location;
            DirectoryInfo execParentDir = Directory.GetParent(execDir);

            // When running tests LanguageFiles is in same directory
            if (execParentDir != null && Directory.Exists(Path.Combine(execParentDir.FullName, "LanguageFiles")))
            {
                return execParentDir.FullName;
            }

            // NitroxModel, NitroxServer and other assemblies are stored in Nitrox.Launcher/lib
            if (execParentDir?.Parent != null && Directory.Exists(Path.Combine(execParentDir.Parent.FullName, "Resources", "LanguageFiles")))
            {
                return execParentDir.Parent.FullName;
            }

            return null;
        },
        () =>
        {
            using ProcessEx proc = ProcessEx.GetFirstProcess("Nitrox.Launcher");
            string executable = proc?.MainModule?.FileName;
            return !string.IsNullOrWhiteSpace(executable) ? Path.GetDirectoryName(executable) : null;
        }
    };

    public static string AppDataPath
    {
        get
        {
            if (appDataPath != null)
            {
                return appDataPath;
            }

            string applicationData = null;

            // On linux Environment.SpecialFolder.ApplicationData returns the Windows version inside wine, this bypasses that behaviour
            string homeInWineEnv = Environment.GetEnvironmentVariable("WINEHOMEDIR");
            if (homeInWineEnv is { Length: > 4 })
            {
                string homeInWine = homeInWineEnv[4..]; // WINEHOMEDIR is prefixed with \??\
                if (Directory.Exists(homeInWine))
                {
                    applicationData = Path.Combine(homeInWine, ".config");
                    Directory.CreateDirectory(applicationData); // Create it if it's not there (which should not happen in normal setups)
                }
            }

            if (!Directory.Exists(applicationData))
            {
                applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            return appDataPath = Path.Combine(applicationData, "Nitrox");
        }
    }

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

    public static string AssetBundlePath => Path.Combine(LauncherPath, "Resources", "AssetBundles");
    public static string LanguageFilesPath => Path.Combine(LauncherPath, "Resources", "LanguageFiles");

    public static string PreferredGamePath
    {
        get => KeyValueStore.Instance.GetValue<string>(PREFERRED_GAMEPATH_KEY);
        set => KeyValueStore.Instance.SetValue(PREFERRED_GAMEPATH_KEY, value);
    }

    private static IGamePlatform gamePlatform;
    public static event Action GamePlatformChanged;

    public static IGamePlatform GamePlatform
    {
        get
        {
            if (gamePlatform == null)
            {
                _ = GamePath; // Ensure gamePath is set
            }
            return gamePlatform;
        }
        set
        {
            if (gamePlatform != value)
            {
                gamePlatform = value;
                GamePlatformChanged?.Invoke();
            }
        }
    }

    public static string GamePath
    {
        get
        {
            if (!string.IsNullOrEmpty(gamePath))
            {
                return gamePath;
            }

            List<GameFinderResult> finderResults = GameInstallationFinder.Instance.FindGame(GameInfo.Subnautica).TakeUntilInclusive(r => r is { IsOk: false }).ToList();
            GameFinderResult potentiallyValidResult = finderResults.LastOrDefault();
            if (potentiallyValidResult?.IsOk == true)
            {
                Log.Debug($"Game installation was found by {potentiallyValidResult.FinderName} at '{potentiallyValidResult.Path}'");
                gamePath = potentiallyValidResult.Path;
                GamePlatform = GamePlatforms.GetPlatformByGameDir(gamePath);
                return gamePath;
            }

            Log.Error($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, finderResults.Select(i => $"{i.FinderName}: {i.ErrorMessage}"))}");
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

    public static string ExecutableRootPath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(executableRootPath))
            {
                return executableRootPath;
            }
            string exePath = ExecutableFilePath;
            if (exePath == null)
            {
                return null;
            }

            return executableRootPath = Path.GetDirectoryName(exePath);
        }
    }

    public static string ExecutableFilePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                return executablePath;
            }

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                return null;
            }
            string path = entryAssembly.Location;
            // File URI works different on Linux/OSX, so only do uri parsing on Windows.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = new Uri(path).LocalPath;
            }
            return executablePath = path;
        }
    }

    public static string AssetsPath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(assetsPath))
            {
                return assetsPath;
            }

            string nitroxAssets;
            if (NitroxEnvironment.IsTesting)
            {
                nitroxAssets = Directory.GetCurrentDirectory();
                while (nitroxAssets != null && Path.GetFileName(nitroxAssets) != "Nitrox.Test")
                {
                    nitroxAssets = Directory.GetParent(nitroxAssets)?.FullName;
                }
                if (nitroxAssets != null)
                {
                    nitroxAssets = Path.Combine(Directory.GetParent(nitroxAssets)?.FullName ?? throw new Exception("Failed to get Nitrox assets during tests"), "Nitrox.Assets.Subnautica");
                }
            }
            else
            {
                nitroxAssets = LauncherPath ?? ExecutableRootPath;
            }
            return assetsPath = nitroxAssets;
        }
    }
}
