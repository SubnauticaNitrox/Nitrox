using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
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
            if (packet.Absolute)
            {
                // In this case we don't want to notify other clients as they already processed the modifications on their own
                baseManager.BasePieceForceDeconstruct(packet.Id);
                return;
            }
            baseManager.BasePieceDeconstructionCompleted(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
