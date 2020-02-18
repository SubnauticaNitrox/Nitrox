using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
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
