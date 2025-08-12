using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Helper;
using static NitroxModel.Discovery.InstallationFinders.Core.GameFinderResult;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
///     Tries to read a local config value that contains the installation directory.
/// </summary>
public sealed class ConfigFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string path = NitroxUser.PreferredGamePath;

        if (string.IsNullOrEmpty(path))
        {
            return Error("Configured game path was found empty. Please enter the path to the game installation");
        }

        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            return Error($"Game installation directory config '{path}' is invalid. Please enter the path to the '{gameInfo.Name}' installation");
        }

        return Ok(path);
    }
}
