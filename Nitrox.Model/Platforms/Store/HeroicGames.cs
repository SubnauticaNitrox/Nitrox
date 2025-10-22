using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public sealed class HeroicGames : IGamePlatform
{
    public string Name => "Heroic Games Launcher";
    public Platform Platform => Platform.HEROIC;

    public bool OwnsGame(string gameDirectory)
    {
        HeroicGamesFinder finder = new();

        // TODO: Limit "FindGame" calls on last folder name of the path.

        // Heroic Games has no files inside or in the parent of the game directory
        GameFinderResult? resultSubnautica = finder.FindGame(GameInfo.Subnautica);
        if (resultSubnautica.IsOk && PathEquals(gameDirectory, resultSubnautica.Path))
        {
            return true;
        }

        GameFinderResult? resultBelowZero = finder.FindGame(GameInfo.SubnauticaBelowZero);
        if (resultBelowZero.IsOk && PathEquals(gameDirectory, resultBelowZero.Path))
        {
            return true;
        }

        return false;

        static string PathNormalize(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        static bool PathEquals(string path1, string path2) => string.Equals(PathNormalize(path1), PathNormalize(path2), StringComparison.Ordinal);
    }

    public static async Task<ProcessEx> StartGameAsync(string egsNamespace, string launchArguments)
    {
        StringBuilder launchCmd = new("heroic://launch?runner=legendary");
        launchCmd.Append("&appName=").Append(egsNamespace);

        string launcherPathPrefix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? string.Empty : "Z:";
        launchCmd.Append("&arg=--nitrox&arg=").Append(launcherPathPrefix).Append(NitroxUser.LauncherPath);

        foreach (string argumentPart in launchArguments.Split(' '))
        {
            if (!string.IsNullOrWhiteSpace(argumentPart))
            {
                launchCmd.Append("&arg=").Append(argumentPart.Trim());
            }
        }

        ProcessStartInfo startInfo;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Need to replace \ with / so it's not eaten as an escape char
            startInfo = new ProcessStartInfo(launchCmd.ToString().Replace('\\', '/'))
            {
                UseShellExecute = true
            };
        }
        else
        {
            startInfo = new ProcessStartInfo("xdg-open", $"\"{launchCmd}\"");
        }

        Log.Info($"Starting game with heroic uri: {startInfo.FileName} {startInfo.Arguments}");
        return await Task.FromResult(ProcessEx.From(startInfo));
    }
}
