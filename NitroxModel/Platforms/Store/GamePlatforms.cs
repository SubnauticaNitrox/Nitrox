using System.IO;
using NitroxModel.Platforms.Store.Interfaces;

namespace NitroxModel.Platforms.Store
{
    public static class GamePlatforms
    {
        public static readonly IGamePlatform[] AllPlatforms = { Steam.Instance, EpicGames.Instance, Discord.Instance, MSStore.Instance };

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
}
