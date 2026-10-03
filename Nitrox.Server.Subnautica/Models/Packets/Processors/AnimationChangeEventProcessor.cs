using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Broadcasts and stores the animation state of a player
/// </summary>
internal sealed class AnimationChangeEventProcessor(PlayerService playerService) : IAuthPacketProcessor<AnimationChangeEvent>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, AnimationChangeEvent packet)
    {
        playerService.GetProperty<ContextProperty>(context.Sender).Value.Animation = packet.Animation;
        await context.SendToOthersAsync(packet);
    }
}
