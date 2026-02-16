using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Broadcasts and stores the animation state of a player
/// </summary>
internal sealed class AnimationChangeEventProcessor : IAuthPacketProcessor<AnimationChangeEvent>
{
    public async Task Process(AuthProcessorContext context, AnimationChangeEvent packet)
    {
        context.Sender.PlayerContext.Animation = packet.Animation;
        await context.SendToOthersAsync(packet);
    }
}
