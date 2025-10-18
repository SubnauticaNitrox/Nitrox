extern alias JB;

namespace NitroxModel.Platforms.Discovery.InstallationFinders.Core;

public interface IGameFinder
{
    /// <summary>
    ///     Searches for game installation directory.
    /// </summary>
    /// <param name="gameInfo">Game to search for.</param>
    /// <returns>Nullable game installation</returns>
    GameFinderResult FindGame(GameInfo gameInfo);
}
