using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record AnimationProperty : IConnectedPlayerProperty<PlayerAnimation, AnimationProperty>
{
    public PlayerAnimation Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    } = new(AnimChangeType.UNDERWATER, AnimChangeState.ON);
}
