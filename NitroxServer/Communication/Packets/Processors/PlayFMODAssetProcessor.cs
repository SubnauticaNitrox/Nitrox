using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class PlayFMODAssetProcessor : AuthenticatedPacketProcessor<PlayFMODAsset>
{
    private readonly PlayerManager playerManager;

    public PlayFMODAssetProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayFMODAsset packet, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            float distance = player.Position.Distance(packet.Position);
            if (player != sendingPlayer &&
                (packet.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) &&
                distance <= packet.Radius)
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, packet.Radius, packet.Volume);
                player.SendPacket(packet);
            }
        }
    }
}
