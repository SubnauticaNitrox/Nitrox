using NitroxModel.Discovery.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
///     Trying to find the path in environment variables by the key {GAMEINFO FULLNAME}_INSTALLATION_PATH that contains the installation directory.
/// <para>
///     SUBNAUTICA_INSTALLATION_PATH,
///     SUBNAUTICAZERO_INSTALLATION_PATH
/// </para>
/// </summary>
public class EnvironmentGameFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        string path = Environment.GetEnvironmentVariable($"{gameInfo.FullName.ToUpper()}_INSTALLATION_PATH");
        if (string.IsNullOrEmpty(path))
        {
            errors.Add(@"Configured game path with environment variable SUBNAUTICA_INSTALLATION_PATH was found empty.");
            return null;
        }

        if (!Directory.Exists(Path.Combine(path, gameInfo.DataFolder, "Managed")))
        {
            errors.Add($@"Game installation directory config '{path}' is invalid. Please enter the path to the game installation.");
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
