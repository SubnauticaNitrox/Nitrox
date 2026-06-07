using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Model.Platforms.Store;

public static class Standalone
{
    public static Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Path.GetExtension(pathToGameExe).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(StartWindowsGameWithWine(pathToGameExe, launchArguments));
        }

        return Task.FromResult(
            ProcessEx.Start(
                pathToGameExe,
                [(NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            )
        );
    }

    private static ProcessEx StartWindowsGameWithWine(string pathToGameExe, string launchArguments)
    {
        string wineExe = FindExecutableOnPath("wine64") ?? FindExecutableOnPath("wine");
        if (wineExe == null)
        {
            throw new FileNotFoundException("Wine was not found. Install Wine, or start the Windows Subnautica executable manually from your Wine wrapper.");
        }

        List<(string, string)> environmentVariables =
        [
            (NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)
        ];
        string? winePrefix = InferWinePrefix(pathToGameExe);
        if (!string.IsNullOrWhiteSpace(winePrefix) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINEPREFIX")))
        {
            environmentVariables.Add(("WINEPREFIX", winePrefix));
        }

        return ProcessEx.Start(
            wineExe,
            environmentVariables,
            Path.GetDirectoryName(pathToGameExe),
            $"\"{pathToGameExe}\" {launchArguments}"
        );
    }

    private static string? InferWinePrefix(string windowsExePath)
    {
        string[] pathParts = windowsExePath.Split(Path.DirectorySeparatorChar);
        int driveCIndex = Array.FindIndex(pathParts, static part => part.Equals("drive_c", StringComparison.OrdinalIgnoreCase));
        if (driveCIndex <= 0)
        {
            return null;
        }

        string prefix = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts.Take(driveCIndex));
        return Directory.Exists(prefix) ? prefix : null;
    }

    private static string? FindExecutableOnPath(string executableName)
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (string path in pathValue.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            string executablePath = Path.Combine(path, executableName);
            if (File.Exists(executablePath))
            {
                return executablePath;
            }
        }

        return null;
    }
}
