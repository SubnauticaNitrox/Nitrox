using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class VehicleRemovePacketProcessor : AuthenticatedPacketProcessor<VehicleDestroyed>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleRemovePacketProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleDestroyed packet, Player player)
        {
            vehicleManager.RemoveVehicle(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
