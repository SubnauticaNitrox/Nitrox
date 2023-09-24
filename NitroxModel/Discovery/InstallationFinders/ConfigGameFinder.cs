using System.Collections.Generic;
using System.IO;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
///     Tries to read a local config value that contains the installation directory.
/// </summary>
public class ConfigGameFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        string path = NitroxUser.PreferredGamePath;

        if (string.IsNullOrEmpty(path))
        {
            errors.Add("Configured game path was found empty. Please enter the path to the game installation.");
            return null;
        }

        if (!Directory.Exists(Path.Combine(path, gameInfo.DataFolder, "Managed")))
        {
            errors.Add($"Game installation directory config '{path}' is invalid. Please enter the path to the game installation.");
            return null;
        }

        if (!GameInstallationFinder.HasGameExecutable(path, gameInfo))
        {
            errors.Add($"Found valid installation directory at '{path}'. But game exe is missing.");
            return null;
        }

        return new()
        {
            Path = path,
            GameInfo = gameInfo,
            Origin = GameLibraries.CONFIG
        };
    }
}
