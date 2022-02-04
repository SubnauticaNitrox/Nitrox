using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class PlayFMODEventInstanceProcessor : AuthenticatedPacketProcessor<PlayFMODEventInstance>
{
    private readonly PlayerManager playerManager;

    public PlayFMODEventInstanceProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayFMODEventInstance packet, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            float distance = player.Position.Distance(packet.Position);
            if (player != sendingPlayer &&
                (packet.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) &&
                ((packet.Play && distance <= packet.Radius) || !packet.Play))
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, packet.Radius, packet.Volume);
                player.SendPacket(packet);
            }
        }
    }
}
