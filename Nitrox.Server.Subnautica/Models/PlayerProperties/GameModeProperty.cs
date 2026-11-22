using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed class GameModeProperty(IOptions<SubnauticaServerOptions> options) : IPlayerProperty<SubnauticaGameMode>
{
    public SubnauticaGameMode Value
    {
        get => Interlocked.CompareExchange(ref field, SubnauticaGameMode.CREATIVE, SubnauticaGameMode.CREATIVE);
        set => Interlocked.Exchange(ref field, value);
    } = options.Value.GameMode;
}
