using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class SignChangedPacketProcessor : AuthenticatedPacketProcessor<SignChanged>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public SignChangedPacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(SignChanged packet, Player player)
        {
            SignMetadata signMetadata = packet.SignMetadata;
            baseData.UpdateBasePieceMetadata(signMetadata.Guid, signMetadata);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
