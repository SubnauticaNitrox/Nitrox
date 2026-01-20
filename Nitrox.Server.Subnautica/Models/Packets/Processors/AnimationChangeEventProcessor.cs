using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Broadcasts and stores the animation state of a player
/// </summary>
internal sealed class AnimationChangeEventProcessor : AuthenticatedPacketProcessor<AnimationChangeEvent>
{
    private readonly IPacketSender packetSender;
    private readonly PlayerManager playerManager;

    public AnimationChangeEventProcessor(IPacketSender packetSender, PlayerManager playerManager)
    {
        this.packetSender = packetSender;
        this.playerManager = playerManager;
    }

    public override void Process(AnimationChangeEvent packet, Player player)
    {
        player.PlayerContext.Animation = packet.Animation;

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
