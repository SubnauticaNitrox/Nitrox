using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BasePieceMetadataChangedPacketProcessor : AuthenticatedPacketProcessor<BasePieceMetadataChanged>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;

        public BasePieceMetadataChangedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(BasePieceMetadataChanged packet, Player player)
        {
            baseManager.UpdateBasePieceMetadata(packet.BaseParentId, packet.PieceId, packet.Metadata);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
