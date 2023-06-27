using NitroxModel.Discovery.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
///     Trying to find the path in environment variables by the key SUBNAUTICA_INSTALLATION_PATH that contains the installation directory.
/// </summary>
public class EnvironmentGameFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        // Allow multiple environment variable by game
        string path = Environment.GetEnvironmentVariable("SUBNAUTICA_INSTALLATION_PATH");
        if (string.IsNullOrEmpty(path))
        {
            errors.Add(@"Configured game path with environment variable SUBNAUTICA_INSTALLATION_PATH was found empty.");
            return null;
        }

        if (!Directory.Exists(Path.Combine(path, gameInfo.DataFolder, "Managed")))
        {
            errors.Add($@"Game installation directory config '{path}' is invalid. Please enter the path to the Subnautica installation.");
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
            Origin = GameLibraries.ENVIRONMENT
        };
    }
}
