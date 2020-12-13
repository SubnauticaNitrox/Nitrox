using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class VehicleMovementPacketProcessor : AuthenticatedPacketProcessor<VehicleMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleMovementPacketProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleMovement packet, Player player)
        {
            vehicleManager.UpdateVehicle(packet.VehicleMovementData);

            if (player.Id == packet.PlayerId)
            {
                player.Position = packet.Position;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
