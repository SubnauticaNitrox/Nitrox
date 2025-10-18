using Nitrox.Model.Serialization;
using Nitrox.Model.Server;

namespace Nitrox.Model.Extensions;

public static class SubnauticaServerConfigExtensions
{
    public static bool IsHardcore(this SubnauticaServerConfig config) => config.GameMode == NitroxGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerConfig config) => config.ServerPassword != "";
}
