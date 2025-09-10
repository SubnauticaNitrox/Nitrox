using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

/// <summary>
/// Broadcasts and stores the animation state of a player
/// </summary>
public class AnimationChangeEventProcessor : AuthenticatedPacketProcessor<AnimationChangeEvent>
{
    private readonly PlayerManager playerManager;

    public AnimationChangeEventProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(AnimationChangeEvent packet, Player player)
    {
        player.PlayerContext.Animation = packet.Animation;

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
