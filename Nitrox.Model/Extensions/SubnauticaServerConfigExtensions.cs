using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;

namespace Nitrox.Model.Extensions;

public static class SubnauticaServerConfigExtensions
{
    public static bool IsHardcore(this SubnauticaServerConfig config) => config.GameMode == SubnauticaGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerConfig config) => config.ServerPassword != "";
}
