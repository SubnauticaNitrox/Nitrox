using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BaseConstructionBeginPacketProcessor : AuthenticatedPacketProcessor<BaseConstructionBegin>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public BaseConstructionBeginPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(BaseConstructionBegin packet, Player player)
        {
            baseManager.BasePieceConstructionBegin(packet.BasePiece);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
