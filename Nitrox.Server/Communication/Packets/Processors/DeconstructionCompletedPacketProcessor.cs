using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class DeconstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<DeconstructionCompleted>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public DeconstructionCompletedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(DeconstructionCompleted packet, Player player)
        {
            baseManager.BasePieceDeconstructionCompleted(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
