using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class SubRootChangedPacketProcessor : AuthenticatedPacketProcessor<SubRootChanged>
    {
        private readonly PlayerManager playerManager;

        public SubRootChangedPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(SubRootChanged packet, Player player)
        {
            Log.Info(packet);
            player.SubRootId = packet.SubRootId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
