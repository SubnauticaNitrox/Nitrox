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

    private static bool TryGetGameInstallation(string path, GameInfo gameInfo, out GameInstallationLayout layout)
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
                layout = new(rootPath, nativeMacExecutable);
                return true;
            }
        }

        string hostExecutable = Path.Combine(rootPath, gameInfo.ExeName);
        string hostManagedPath = Path.Combine(rootPath, gameInfo.DataFolder, "Managed");
        if (File.Exists(hostExecutable) && Directory.Exists(hostManagedPath))
        {
            layout = new(rootPath, hostExecutable);
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

    private sealed record GameInstallationLayout(string RootPath, string ExecutablePath);
}
