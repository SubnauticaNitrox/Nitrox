using System.IO;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public static class GamePlatforms
{
    public static readonly IGamePlatform[] AllPlatforms = [new Steam(), new EpicGames(), new Discord(), new MSStore()];

    public static IGamePlatform GetPlatformByGameDir(string gameDirectory)
    {
        if (!Directory.Exists(gameDirectory))
        {
            return null;
        }

        foreach (IGamePlatform platform in AllPlatforms)
        {
            if (platform.OwnsGame(gameDirectory))
            {
                return platform;
            }
        }

        return null;
    }
}
