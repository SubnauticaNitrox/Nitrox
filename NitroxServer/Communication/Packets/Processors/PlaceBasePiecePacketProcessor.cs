using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlaceBasePiecePacketProcessor : AuthenticatedPacketProcessor<PlaceBasePiece>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        
        public PlaceBasePiecePacketProcessor(BaseData baseData, PlayerManager playerManager)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
        }

        public override void Process(PlaceBasePiece packet, Player player)
        {
            baseData.AddBasePiece(packet.BasePiece);
            playerManager.SendPacketToOtherPlayers(packet, player);
            NitroxModel.Logger.Log.Info(packet.ToString()); //#gitignore
        }
    }
}
