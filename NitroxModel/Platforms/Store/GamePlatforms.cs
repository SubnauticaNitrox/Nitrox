using System.IO;
using NitroxModel.Platforms.Store.Interfaces;
using static NitroxServer.Server;
using static NitroxModel.DisplayStatusCodes;
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
                } else
                {
                    DisplayStatusCode(StatusCode.deadPiratesTellNoTales, true);
                }
            }

            return null;
        }
    }
}
