using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class ConstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<ConstructionCompleted>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public ConstructionCompletedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(ConstructionCompleted packet, Player player)
        {
            baseManager.BasePieceConstructionCompleted(packet.PieceId, packet.BaseId);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
