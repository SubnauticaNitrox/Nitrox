using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Extensions;

public static class SubnauticaServerOptionsExtensions
{
    public static bool IsHardcore(this SubnauticaServerOptions options) => options.GameMode == SubnauticaGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerOptions options) => options.ServerPassword != "";
}
