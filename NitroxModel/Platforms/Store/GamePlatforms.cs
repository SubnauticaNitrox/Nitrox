using System.IO;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store;

public static class GamePlatforms
{
    public static readonly IGamePlatform[] AllPlatforms = [Steam.Instance, EpicGames.Instance, Discord.Instance, MSStore.Instance];

    public static IGamePlatform GetPlatformByGameDir(string gameDirectory)
    {
        Log.Warn($"Platform start: '{gameDirectory}'");
        if (!Directory.Exists(gameDirectory))
        {
            Log.Warn($"Platform dir not exist: '{gameDirectory}'");
            return null;
        }

        foreach (IGamePlatform platform in AllPlatforms)
        {
            Log.Warn($"Does platform '{platform.Name}' owns '{gameDirectory}'?: '{(platform.OwnsGame(gameDirectory) ? "true" : "false")}'");
            if (platform.OwnsGame(gameDirectory))
            {
                return platform;
            }
        }

        return null;
    }
}
