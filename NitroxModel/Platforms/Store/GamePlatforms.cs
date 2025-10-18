using System.Collections.Generic;
using System.IO;
using NitroxModel.Platforms.Discovery.Models;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public static class GamePlatforms
{
    private static readonly Dictionary<GameLibraries, IGamePlatform> allPlatforms = new()
    {
        { GameLibraries.STEAM, new Steam() },
        { GameLibraries.EPIC, new EpicGames() },
        { GameLibraries.HEROIC, new HeroicGames() },
        { GameLibraries.MICROSOFT, new MSStore() },
        { GameLibraries.DISCORD, new Discord() }
    };

    public static IGamePlatform? GetPlatformByFlag(GameLibraries gameLibraries) => allPlatforms.GetValueOrDefault(gameLibraries);

    public static IGamePlatform? GetPlatformByGameDir(string gameRootPath)
    {
        if (!Directory.Exists(gameRootPath))
        {
            return null;
        }

        foreach (IGamePlatform platform in allPlatforms.Values)
        {
            if (platform.OwnsGame(gameRootPath))
            {
                return platform;
            }
        }

        return null;
    }
}
