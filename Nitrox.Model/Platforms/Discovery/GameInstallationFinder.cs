using System;
using System.Collections.Generic;
using System.IO;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Model.Platforms.Discovery;

/// <summary>
/// Main game installation finder that will use all available methods of detection to find the game installation directory
/// </summary>
public sealed class GameInstallationFinder
{
    private static readonly Lazy<GameInstallationFinder> instance = new(() => new GameInstallationFinder());
    public static GameInstallationFinder Instance => instance.Value;

    private readonly Dictionary<GameLibraries, IGameFinder> finders = new()
    {
        { GameLibraries.STEAM, new SteamFinder() },
        { GameLibraries.EPIC, new EpicGamesFinder() },
        { GameLibraries.HEROIC, new HeroicGamesFinder() },
        { GameLibraries.MICROSOFT, new MicrosoftFinder() },
        { GameLibraries.DISCORD, new DiscordFinder() },
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
        if (!gameLibraries.IsDefined())
        {
            yield break;
        }

        foreach (GameLibraries wantedFinder in gameLibraries.GetUniqueNonCombinatoryFlags())
        {
            if (!finders.TryGetValue(wantedFinder, out IGameFinder finder))
            {
                continue;
            }

            GameFinderResult result = finder.FindGame(gameInfo);
            if (!result.IsOk && string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result = result with { ErrorMessage = $"It appears you don't have {gameInfo.Name} installed" };
            }
            if (result.Origin == default)
            {
                result = result with { Origin = wantedFinder };
            }
            if (!string.IsNullOrEmpty(result.Path))
            {
                result = result with { Path = Path.GetFullPath(result.Path) };
            }
            yield return result;
        }
    }
}
