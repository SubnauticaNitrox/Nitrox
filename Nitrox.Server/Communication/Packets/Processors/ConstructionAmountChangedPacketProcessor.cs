using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class ConstructionAmountChangedPacketProcessor : AuthenticatedPacketProcessor<ConstructionAmountChanged>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public ConstructionAmountChangedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(ConstructionAmountChanged packet, Player player)
        {
            baseManager.BasePieceConstructionAmountChanged(packet.Id, packet.ConstructionAmount);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
