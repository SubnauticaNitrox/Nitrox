using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.Serialization.World;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class VehicleSpawnedProcessor : AuthenticatedPacketProcessor<VehicleSpawned>
    {
        private readonly PlayerManager playerManager;
        private readonly World world;

        public VehicleSpawnedProcessor(PlayerManager playerManager, World world)
        {
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(VehicleSpawned packet, Player player)
        {
            world.VehicleManager.AddVehicle(packet.VehicleModel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
