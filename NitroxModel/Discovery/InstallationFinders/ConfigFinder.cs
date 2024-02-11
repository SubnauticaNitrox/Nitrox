using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
/// Tries to read a local config value that contains the installation directory.
/// </summary>
public sealed class ConfigFinder : IGameFinder
{
    public GameInstallation FindGame(GameInfo gameInfo, List<string> errors)
    {
        string path = NitroxUser.PreferredGamePath;

        if (string.IsNullOrEmpty(path))
        {
            errors.Add("Configured game path was found empty. Please enter the path to the game installation");
            return null;
        }

        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            errors.Add($"Game installation directory config '{path}' is invalid. Please enter the path to the '{gameInfo.Name}' installation");
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
