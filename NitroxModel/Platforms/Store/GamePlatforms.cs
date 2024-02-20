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
                    DisplayStatusCode(StatusCode.deadPiratesTellNoTales, true, "Please ask for support in the nitrox discord using the join button.A member of the support team would be glad to assist you if you provide them with this status code.");
                }
            }

            return null;
        }
    }
}
