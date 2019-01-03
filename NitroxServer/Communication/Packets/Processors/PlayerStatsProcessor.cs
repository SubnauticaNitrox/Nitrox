using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Serialization.World;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerStatsProcessor : AuthenticatedPacketProcessor<PlayerStats>
    {
        private readonly World world;
        private readonly PlayerManager playerManager;

        public PlayerStatsProcessor(World world, PlayerManager playerManager)
        {
            this.world = world;
            this.playerManager = playerManager;
        }

        public override void Process(PlayerStats packet, Player player)
        {
            world.PlayerData.PlayerStats(player.Name, packet);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
