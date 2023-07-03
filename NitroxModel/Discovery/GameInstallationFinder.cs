using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Discovery.InstallationFinders;
using NitroxModel.Discovery.Models;

namespace NitroxModel.Discovery;

/// <summary>
///     Main game installation finder that will use all available methods of detection to find the game installation directory.
/// </summary>
public class GameInstallationFinder
{
    private static readonly Lazy<GameInstallationFinder> instance = new(() => new GameInstallationFinder());
    public static GameInstallationFinder Instance => instance.Value;

    private readonly Dictionary<GameLibraries, IGameFinder> finders = new()
    {
        { GameLibraries.STEAM, new SteamGameRegistryFinder() },
        { GameLibraries.EPIC, new EpicGamesInstallationFinder() },
        { GameLibraries.DISCORD, new DiscordGameFinder() },
        { GameLibraries.MICROSOFT, new DefaultFinder() },
        { GameLibraries.ENVIRONMENT, new EnvironmentGameFinder() },
        { GameLibraries.CURRENT_DIRECTORY, new GameInCurrentDirectoryFinder() },
        { GameLibraries.CONFIG, new ConfigGameFinder() }
    };

    public IEnumerable<GameInstallation> FindGame(GameInfo gameInfo, GameLibraries gameLibraries, IList<string> errors)
    { 
        errors ??= new List<string>();

        if (gameInfo == null || !Enum.IsDefined(typeof(GameLibraries), gameLibraries))
        {
            yield break;
        }

        foreach (GameLibraries wantedFinder in GetFlags(gameLibraries))
        {
            if (!finders.TryGetValue(wantedFinder, out IGameFinder finder))
            {
                errors.Add($"Could not find game finder for configuration : '{wantedFinder}'");
                continue;
            }

            GameInstallation? finderResult = finder.FindGame(gameInfo, errors);
            if (finderResult.HasValue)
            {
                yield return finderResult.Value;
            }
        }
    }

    public static T[] GetFlags<T>(T flagsEnumValue) where T : Enum
    {
        return Enum
            .GetValues(typeof(T))
            .Cast<T>()
            .Where(e => flagsEnumValue.HasFlag(e))
            .ToArray();
    }

    public static bool HasGameExecutable(string path, GameInfo gameInfo)
    {
        return File.Exists(Path.Combine(path, gameInfo.ExeName));
    }
}
