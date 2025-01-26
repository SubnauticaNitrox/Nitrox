using System;
using NitroxModel.Discovery.InstallationFinders.Core;
using static NitroxModel.Discovery.InstallationFinders.Core.GameFinderResult;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
/// Trying to find the path in environment variables by the key {GAMEINFO FULLNAME}_INSTALLATION_PATH that contains the installation directory.
/// <list>
///     <item>SUBNAUTICA_INSTALLATION_PATH</item>
///     <item>SUBNAUTICAZERO_INSTALLATION_PATH</item>
/// </list>
/// </summary>
public sealed class EnvironmentFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string path = Environment.GetEnvironmentVariable($"{gameInfo.Name.ToUpper()}_INSTALLATION_PATH");
        if (string.IsNullOrEmpty(path))
        {
            return Error($"Configured game path with environment variable {gameInfo.Name.ToUpper()}_INSTALLATION_PATH was found empty");
        }

        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            return Error($"Game installation directory '{path}' is invalid. Please enter the path to the '{gameInfo.Name}' installation");
        }

        return Ok(path);
    }
}
