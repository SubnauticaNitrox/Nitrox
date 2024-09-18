using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;
public class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
{
    private readonly float footstepAudioRange = 20f;
    private readonly PlayerManager playerManager;

    public FootstepPacketProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(FootstepPacket footstepPacket, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) <= footstepAudioRange && player != sendingPlayer)
            {
                // Forward footstep packet to players if they are within range to hear it and are in the same structure / submarine
                if (sendingPlayer.SubRootId.HasValue && player.SubRootId.HasValue &&
                    sendingPlayer.SubRootId.Value == player.SubRootId.Value)
                {
                    player.SendPacket(footstepPacket);
                }
                else if (!sendingPlayer.SubRootId.HasValue && !player.SubRootId.HasValue)
                {
                    player.SendPacket(footstepPacket);
                }
            }
        }
    }
}
