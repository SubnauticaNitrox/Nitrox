using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerStatsProcessor : AuthenticatedPacketProcessor<PlayerStats>
    {
        private readonly World world;

        public PlayerStatsProcessor(World world)
        {
            this.world = world;
        }

        public override void Process(PlayerStats packet, Player player)
        {
            world.PlayerData.PlayerStats(player.Name, packet);
        }
    }
}
