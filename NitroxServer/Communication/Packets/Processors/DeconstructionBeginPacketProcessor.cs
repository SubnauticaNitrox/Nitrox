using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DeconstructionBeginPacketProcessor : AuthenticatedPacketProcessor<DeconstructionBegin>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public DeconstructionBeginPacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(DeconstructionBegin packet, Player player)
        {
            baseData.BasePieceDeconstructionBegin(packet.Guid);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
