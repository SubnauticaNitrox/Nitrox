using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
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

        public override void Process(ConstructorBeginCrafting packet, NitroxServer.Player player)
        {
            vehicleManager.AddVehicle(packet.VehicleModel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
