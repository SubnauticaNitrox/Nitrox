using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class DeconstructionBeginPacketProcessor : AuthenticatedPacketProcessor<DeconstructionBegin>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public DeconstructionBeginPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(DeconstructionBegin packet, Player player)
        {
            baseManager.BasePieceDeconstructionBegin(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
