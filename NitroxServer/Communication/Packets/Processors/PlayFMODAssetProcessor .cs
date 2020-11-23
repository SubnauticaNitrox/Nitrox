using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
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
                float distance = NitroxVector3.Distance(player.Position, packet.Position);
                if (player != sendingPlayer && distance <= packet.Radius)
                {
                    packet.Volume = 1 - distance / packet.Radius;
                    player.SendPacket(packet);
                }
            }
        }
    }
}
