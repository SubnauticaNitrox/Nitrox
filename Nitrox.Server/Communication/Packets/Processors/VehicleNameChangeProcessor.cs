using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class VehicleNameChangeProcessor : AuthenticatedPacketProcessor<VehicleNameChange>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleNameChangeProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleNameChange packet, Player player)
        {
            vehicleManager.UpdateVehicleName(packet.Id, packet.Name);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
