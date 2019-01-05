using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ConstructionAmountChangedPacketProcessor : AuthenticatedPacketProcessor<ConstructionAmountChanged>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public ConstructionAmountChangedPacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(ConstructionAmountChanged packet, Player player)
        {
            baseData.BasePieceConstructionAmountChanged(packet.Guid, packet.ConstructionAmount);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
