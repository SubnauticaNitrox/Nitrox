using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerDeathAnimationProcessor : IAuthPacketProcessor<PlayerDeathAnimation>
{
    public Task Process(AuthProcessorContext context, PlayerDeathAnimation packet)
    {
        if (packet.AnimationType is not (PlayerDeathAnimationType.NORMAL or PlayerDeathAnimationType.FIRE or PlayerDeathAnimationType.EXPLOSION))
        {
            return Task.CompletedTask;
        }

        context.Sender.PlayerContext.AnimatorParameters.Clear();
        return context.SendToOthersAsync(new PlayerDeathAnimation(context.Sender.PlayerContext.SessionId, packet.AnimationType));
    }
}
