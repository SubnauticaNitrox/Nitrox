using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EscapePodChangedPacketProcessor : AuthenticatedPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager playerManager;

        public EscapePodChangedPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(EscapePodChanged packet, Player player)
        {
            Log.Debug(packet);
            player.SubRootId = packet.EscapePodId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
