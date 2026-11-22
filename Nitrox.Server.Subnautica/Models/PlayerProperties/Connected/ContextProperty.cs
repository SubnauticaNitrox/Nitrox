using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed class ContextProperty : IConnectedPlayerProperty<PlayerContext, ContextProperty>
{
    public PlayerContext Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    }
}
