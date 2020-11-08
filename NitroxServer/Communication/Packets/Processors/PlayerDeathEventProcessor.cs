using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerDeathEventProcessor : AuthenticatedPacketProcessor<PlayerDeathEvent>
    {
        private readonly PlayerManager playerManager;
        private readonly ServerProperties serverConfig;

        public PlayerDeathEventProcessor(PlayerManager playerManager, ServerProperties Properties)
        {
            this.playerManager = playerManager;
            this.serverConfig = Properties;
        }

        public override void Process(PlayerDeathEvent packet, Player player)
        {
            if(serverConfig.IsHardcore)
            {
                player.IsPermaDeath = true;
                PlayerKicked playerKicked = new PlayerKicked("Permanent death from hardcore mode");
                player.SendPacket(playerKicked);
            }

            player.LastStoredPosition = packet.DeathPosition;

            if (player.Permissions > Perms.MODERATOR)
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You can use /back to go to your death location"));
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
