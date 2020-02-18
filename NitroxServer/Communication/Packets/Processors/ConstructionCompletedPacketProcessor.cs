using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
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
