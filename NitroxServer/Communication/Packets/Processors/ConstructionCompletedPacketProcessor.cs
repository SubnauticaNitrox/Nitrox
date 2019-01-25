using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ConstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<ConstructionCompleted>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public ConstructionCompletedPacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(ConstructionCompleted packet, Player player)
        {
            baseData.BasePieceConstructionCompleted(packet.Guid, packet.BaseGuid);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
