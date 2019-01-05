using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleMovementPacketProcessor : AuthenticatedPacketProcessor<VehicleMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleMovementPacketProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleMovement packet, Player player)
        {
            vehicleData.UpdateVehicle(packet.Vehicle);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
