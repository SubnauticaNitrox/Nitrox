using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
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
                player.Position = packet.VehicleMovementData.DriverPosition ?? packet.Position;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
