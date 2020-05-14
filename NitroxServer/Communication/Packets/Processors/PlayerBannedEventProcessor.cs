using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerBannedEventProcessor : AuthenticatedPacketProcessor<PlayerDeathEvent>
    {
        private readonly PlayerManager playerManager;

        public PlayerBannedEventProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerDeathEvent packet, Player player)
        {
            player.IsPlayerBanned = true;
            PlayerKicked playerKicked = new PlayerKicked("You have been banned from the server");
            player.SendPacket(playerKicked);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
