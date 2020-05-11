using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BaseDeconstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<BaseDeconstructionCompleted>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public BaseDeconstructionCompletedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(BaseDeconstructionCompleted packet, Player player)
        {
            baseManager.BasePieceDeconstructionCompleted(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
