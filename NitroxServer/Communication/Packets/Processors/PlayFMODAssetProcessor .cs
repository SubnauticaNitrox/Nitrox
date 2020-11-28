using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
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
            Log.Debug($"Received PlayFMODAsset packet: {packet}");
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                float distance = NitroxVector3.Distance(player.Position, packet.Position);
                if (player != sendingPlayer && (packet.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) && distance <= packet.Radius)
                {
                    Log.Debug($"{packet.Volume}:{distance}:{packet.Radius}");
                    packet.Volume -= (distance / packet.Radius) * packet.Volume; // Not realistic volume calculation
                    Log.Debug($"{packet.Volume}");
                    player.SendPacket(packet);
                }
            }
        }
    }
}
