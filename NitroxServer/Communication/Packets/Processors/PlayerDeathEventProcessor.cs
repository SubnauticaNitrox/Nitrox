using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerDeathEventProcessor : AuthenticatedPacketProcessor<PlayerDeathEvent>
    {
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public PlayerDeathEventProcessor(PlayerManager playerManager, ServerConfig serverConfig)
        {
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        public override void Process(PlayerDeathEvent packet, Player player)
        {
            if(serverConfig.IsGameMode("Hardcore"))
            {
                player.IsPermaDeath = true;
                PlayerKicked playerKicked = new PlayerKicked("Permanent death from hardcore mode");
                player.SendPacket(playerKicked);
            }

            player.LastStoredPosition = packet.DeathPosition;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
