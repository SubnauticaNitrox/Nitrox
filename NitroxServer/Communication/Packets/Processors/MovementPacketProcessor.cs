using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private readonly PlayerManager playerManager;
        private readonly World world;

        public MovementPacketProcessor(PlayerManager playerManager, World world)
        {
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(Movement packet, Player player)
        {
            world.PlayerData.UpdatePlayerSpawn(player.Name, packet.Position);
            player.Position = packet.Position;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
