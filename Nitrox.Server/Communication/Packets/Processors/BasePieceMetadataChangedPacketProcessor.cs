using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;

namespace Nitrox.Server.Communication.Packets.Processors
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
            baseManager.UpdateBasePieceMetadata(packet.PieceId, packet.Metadata);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
