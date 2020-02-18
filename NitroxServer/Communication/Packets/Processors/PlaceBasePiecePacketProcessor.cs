using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlaceBasePiecePacketProcessor : AuthenticatedPacketProcessor<PlaceBasePiece>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;
        
        public PlaceBasePiecePacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(PlaceBasePiece packet, Player player)
        {
            baseManager.AddBasePiece(packet.BasePiece);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
