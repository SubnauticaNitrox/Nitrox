using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class MapRoomScanProcessor : AuthenticatedPacketProcessor<MapRoomScan>
    {
        private readonly PlayerManager playerManager;

        public MapRoomScanProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(MapRoomScan packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
