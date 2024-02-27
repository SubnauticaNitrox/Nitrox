using NitroxModel.Discovery.InstallationFinders;
using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Discovery.Models;
using System;
using System.Collections.Generic;

namespace NitroxModel.Discovery;

/// <summary>
/// Main game installation finder that will use all available methods of detection to find the Subnautica installation directory
/// </summary>
public sealed class GameInstallationFinder
{
    private static readonly Lazy<GameInstallationFinder> instance = new(() => new GameInstallationFinder());
    public static GameInstallationFinder Instance => instance.Value;

    private readonly Dictionary<GameLibraries, IGameFinder> finders = new()
    {
        { GameLibraries.STEAM, new SteamFinder() },
        { GameLibraries.EPIC, new EpicGamesFinder() },
        { GameLibraries.DISCORD, new DiscordFinder() },
        { GameLibraries.MICROSOFT, new MicrosoftFinder() },
        { GameLibraries.ENVIRONMENT, new EnvironmentFinder() },
        { GameLibraries.CONFIG, new ConfigFinder() }
    };

    /// <summary>
    ///     Searches for the game install directory given its <see cref="GameInfo"/>.
    /// </summary>
    /// <param name="gameInfo">Info object of a game.</param>
    /// <param name="gameLibraries">Known game libraries to search through</param>
    /// <returns>Positive and negative results from the search</returns>
    public IEnumerable<GameFinderResult> FindGame(GameInfo gameInfo, GameLibraries gameLibraries = GameLibraries.ALL)
    {
        if (gameInfo is null || !Enum.IsDefined(typeof(GameLibraries), gameLibraries))
        {
            yield break;
        }

        foreach (GameLibraries wantedFinder in gameLibraries.GetFlags<GameLibraries>())
        {
            if (!finders.TryGetValue(wantedFinder, out IGameFinder finder))
            {
                continue;
            }

            yield return finder.FindGame(gameInfo);
        }
    }
}
