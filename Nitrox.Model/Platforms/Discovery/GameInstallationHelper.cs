using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nitrox.Model.Platforms.Discovery;

public static class GameInstallationHelper
{
    public static bool HasGameExecutable(string path, GameInfo gameInfo)
    {
        return TryGetGameInstallation(path, gameInfo, out GameInstallationLayout layout) && File.Exists(layout.ExecutablePath);
    }

    public static bool HasValidGameFolder(string path, GameInfo gameInfo)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }
        return TryGetGameInstallation(path, gameInfo, out _);
    }

    public static string NormalizeGamePath(string path, GameInfo gameInfo)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        return TryGetGameInstallation(path, gameInfo, out GameInstallationLayout layout) ? layout.RootPath : Path.GetFullPath(path);
    }

    public static bool TryGetGameExecutablePath(string path, GameInfo gameInfo, out string executablePath)
    {
        if (TryGetGameInstallation(path, gameInfo, out GameInstallationLayout layout))
        {
            executablePath = layout.ExecutablePath;
            return true;
        }

        executablePath = "";
        return false;
    }

    public static bool IsNativeMacOSGameLayout(string path, GameInfo gameInfo)
    {
        return TryGetGameInstallation(path, gameInfo, out GameInstallationLayout layout) && layout.Kind == GameInstallationKind.NativeMacOS;
    }

    public static bool TryGetGameInstallation(string path, GameInfo gameInfo, out GameInstallationLayout layout)
    {
        layout = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string rootPath = Path.GetFullPath(path);
        foreach (string candidatePath in GetCandidateRootPaths(rootPath, gameInfo))
        {
            if (TryCreateLayout(candidatePath, gameInfo, out layout))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryCreateLayout(string rootPath, GameInfo gameInfo, out GameInstallationLayout layout)
    {
        layout = null;
        if (!Directory.Exists(rootPath))
        {
            return false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string nativeMacExecutable = Path.Combine(rootPath, "MacOS", gameInfo.ExeName);
            string nativeMacManagedPath = Path.Combine(rootPath, gameInfo.DataFolder, "Managed");
            if (File.Exists(nativeMacExecutable) && Directory.Exists(nativeMacManagedPath))
            {
                layout = new(rootPath, nativeMacExecutable, nativeMacManagedPath, GameInstallationKind.NativeMacOS);
                return true;
            }
        }

        string hostExecutable = Path.Combine(rootPath, gameInfo.ExeName);
        string hostManagedPath = Path.Combine(rootPath, gameInfo.DataFolder, "Managed");
        if (File.Exists(hostExecutable) && Directory.Exists(hostManagedPath))
        {
            layout = new(rootPath, hostExecutable, hostManagedPath, GameInstallationKind.Host);
            return true;
        }

        return false;
    }

    private static IEnumerable<string> GetCandidateRootPaths(string path, GameInfo gameInfo)
    {
        yield return path;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            yield break;
        }

        if (Path.GetExtension(path).Equals(".app", StringComparison.OrdinalIgnoreCase))
        {
            yield return Path.Combine(path, "Contents");
        }

        yield return Path.Combine(path, $"{gameInfo.Name}.app", "Contents");
    }
}

public sealed record GameInstallationLayout(string RootPath, string ExecutablePath, string ManagedPath, GameInstallationKind Kind);

public enum GameInstallationKind
{
    Host,
    NativeMacOS
}
