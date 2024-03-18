extern alias JB;
using JB::JetBrains.Annotations;

namespace NitroxModel.Discovery.InstallationFinders.Core;

public interface IGameFinder
{
    /// <summary>
    ///     Searches for game installation directory.
    /// </summary>
    /// <param name="gameInfo">Game to search for.</param>
    /// <returns>Nullable game installation</returns>
    [NotNull] GameFinderResult FindGame(GameInfo gameInfo);
}
