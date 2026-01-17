using Nitrox.Model.Configuration;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Extensions;

public static class SubnauticaServerConfigExtensions
{
    public static bool IsHardcore(this SubnauticaServerOptions config) => config.GameMode == SubnauticaGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerOptions config) => config.ServerPassword != "";
}
