using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DeconstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<DeconstructionCompleted>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public DeconstructionCompletedPacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(DeconstructionCompleted packet, Player player)
        {
            baseData.BasePieceDeconstructionCompleted(packet.Guid);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
