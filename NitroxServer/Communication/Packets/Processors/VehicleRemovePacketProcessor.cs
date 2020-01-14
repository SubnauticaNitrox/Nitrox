using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleRemovePacketProcessor : AuthenticatedPacketProcessor<VehicleDestroyed>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleRemovePacketProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleDestroyed packet, Player player)
        {
            vehicleData.RemoveVehicle(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
