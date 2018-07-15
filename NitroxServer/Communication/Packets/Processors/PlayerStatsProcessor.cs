using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;
using NitroxModel.DataStructures.GameLogic;

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
