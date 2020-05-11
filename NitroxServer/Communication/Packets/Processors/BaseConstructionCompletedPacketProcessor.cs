using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BaseConstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<BaseConstructionCompleted>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public BaseConstructionCompletedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(BaseConstructionCompleted packet, Player player)
        {
            baseManager.BasePieceConstructionCompleted(packet.PieceId, packet.BaseId);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
