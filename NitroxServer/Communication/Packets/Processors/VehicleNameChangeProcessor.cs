using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleNameChangeProcessor : AuthenticatedPacketProcessor<VehicleNameChange>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleNameChangeProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleNameChange packet, Player player)
        {
            vehicleData.UpdateVehicleName(packet.Id, packet.Name);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
