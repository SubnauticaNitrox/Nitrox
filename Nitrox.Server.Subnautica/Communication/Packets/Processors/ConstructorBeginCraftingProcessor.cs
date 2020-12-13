using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
{
    class ConstructorBeginCraftingProcessor : AuthenticatedPacketProcessor<ConstructorBeginCrafting>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public ConstructorBeginCraftingProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(ConstructorBeginCrafting packet, Player player)
        {
            vehicleManager.AddVehicle(packet.VehicleModel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
