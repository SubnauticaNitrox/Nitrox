using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class SignChangedPacketProcessor : AuthenticatedPacketProcessor<SignChanged>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public SignChangedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(SignChanged packet, Player player)
        {
            SignMetadata signMetadata = packet.SignMetadata;
            baseManager.UpdateBasePieceMetadata(signMetadata.Id, signMetadata);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
