using System.IO;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store;

public static class GamePlatforms
{
    public static readonly IGamePlatform[] AllPlatforms = [new Steam(), new EpicGames(), new Discord(), new MSStore()];

    public static IGamePlatform GetPlatformByGameDir(string gameRootPath)
    {
        if (!Directory.Exists(gameRootPath))
        {
            return null;
        }

        foreach (IGamePlatform platform in AllPlatforms)
        {
            if (platform.OwnsGame(gameRootPath))
            {
                return platform;
            }
        }

        return null;
    }
}
