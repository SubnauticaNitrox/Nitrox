using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class DockingStateChangeProcessor : AuthenticatedPacketProcessor<DockingStateChange>
    {
        private readonly PlayerManager playerManager;

        public DockingStateChangeProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(DockingStateChange packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
