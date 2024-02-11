using NitroxModel.Discovery.Models;
using System.Collections.Generic;

namespace NitroxModel.Discovery.InstallationFinders.Core;

public interface IGameFinder
{
    /// <summary>
    ///     Searches for game installation directory.
    /// </summary>
    /// <param name="gameInfo">Game to search for.</param>
    /// <param name="errors">Error messages that can be set if it failed to find the game.</param>
    /// <returns>Nullable game installation</returns>
    GameInstallation FindGame(GameInfo gameInfo, List<string> errors);
}
