using NitroxModel.Serialization;
using NitroxModel.Server;

namespace NitroxModel.Extensions;

public static class SubnauticaServerConfigExtensions
{
    public static bool IsHardcore(this SubnauticaServerConfig config) => config.GameMode == NitroxGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerConfig config) => config.ServerPassword != "";
}
