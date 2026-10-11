using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     Broadcasts and stores the animation state of a player
/// </summary>
internal sealed class AnimationChangeEventProcessor(IPlayerPropProvider playerPropProvider) : IAuthPacketProcessor<AnimationChangeEvent>
{
    private readonly IPlayerPropProvider playerPropProvider = playerPropProvider;

    public async Task Process(AuthProcessorContext context, AnimationChangeEvent packet)
    {
        playerPropProvider.Get<ContextProperty>(context.Sender).Value.Animation = packet.Animation;
        await context.SendToOthersAsync(packet);
    }
}
