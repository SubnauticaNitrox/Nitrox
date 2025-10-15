using System.IO;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public static class GamePlatforms
{
    private static readonly IGamePlatform[] allPlatforms = [new Steam(), new EpicGames(), new HeroicGames(), new MSStore(), new Discord()];

    public static IGamePlatform? GetPlatformByGameDir(string gameRootPath)
    {
        if (!Directory.Exists(gameRootPath))
        {
            return null;
        }

        foreach (IGamePlatform platform in allPlatforms)
        {
            if (platform.OwnsGame(gameRootPath))
            {
                return platform;
            }
        }

        return null;
    }
}
