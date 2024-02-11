using NitroxModel.Discovery.InstallationFinders;
using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Discovery.Models;
using System;
using System.Collections.Generic;
using System.IO;

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
    ///     Searches for <see cref="Game"/> directory.
    /// </summary>
    /// <param name="errors">Error messages that can be set if it failed to find the game.</param>
    /// <returns>Nullable path to the Subnautica installation path.</returns>
    public IEnumerable<GameInstallation> FindGame(GameInfo gameInfo, GameLibraries gameLibraries, List<string> errors)
    {
        errors ??= [];

        if (gameInfo is null || !Enum.IsDefined(typeof(GameLibraries), gameLibraries))
        {
            yield break;
        }

        foreach (GameLibraries wantedFinder in gameLibraries.GetFlags<GameLibraries>())
        {
            if (!finders.TryGetValue(wantedFinder, out IGameFinder finder))
            {
                errors.Add($"Could not find game finder for configuration : '{wantedFinder}'");
                continue;
            }

            GameInstallation finderResult = finder.FindGame(gameInfo, errors);
            if (finderResult is not null)
            {
                yield return finderResult;
            }
        }
    }

    public static bool HasGameExecutable(string path, GameInfo gameInfo)
    {
        return File.Exists(Path.Combine(path, gameInfo.ExeName));
    }
}
