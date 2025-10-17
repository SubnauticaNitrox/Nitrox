using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.Store;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Discovery;

/// <summary>
/// Main game installation finder that will use all available methods of detection to find the game installation directory
/// </summary>
public static class GameInstallationFinder
{
    private static readonly Dictionary<GameLibraries, IGameFinder> finders = new()
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
    /// Searches the system for a valid installation of <param name="gameInfo"></param>.
    /// If found saves it to <see cref="NitroxUser.GamePath"/> / <see cref="NitroxUser.GamePlatform"/> and returns true.
    /// </summary>
    public static bool FindPlatformAndGame(GameInfo gameInfo, GameLibraries gameLibraries = GameLibraries.ALL)
    {
        List<GameFinderResult> finderResults = FindGame(gameInfo, gameLibraries).TakeUntilInclusive(r => r is { IsOk: false }).ToList();
        GameFinderResult? potentiallyValidResult = finderResults.LastOrDefault();
        if (potentiallyValidResult is { IsOk: true })
        {
            Log.Debug($"Game installation was found by {potentiallyValidResult.FinderName} at '{potentiallyValidResult.Path}'");
            IGamePlatform platform = GamePlatforms.GetPlatformByFlag(potentiallyValidResult.Origin);
            NitroxUser.SetGamePathAndPlatform(potentiallyValidResult.Path, platform);
            return true;
        }

        Log.Error($"Could not locate {gameInfo.Name} installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, finderResults.Select(i => $"{i.FinderName}: {i.ErrorMessage}"))}");
        return false;
    }

    /// <summary>
    ///     Searches for the game install directory given its <see cref="GameInfo"/>.
    /// </summary>
    /// <param name="gameInfo">Info object of a game.</param>
    /// <param name="gameLibraries">Known game libraries to search through</param>
    /// <returns>Positive and negative results from the search</returns>
    private static IEnumerable<GameFinderResult> FindGame(GameInfo gameInfo, GameLibraries gameLibraries)
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
