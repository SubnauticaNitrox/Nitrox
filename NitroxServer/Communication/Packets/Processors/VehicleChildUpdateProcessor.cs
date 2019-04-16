using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleChildUpdateProcessor : AuthenticatedPacketProcessor<VehicleChildUpdate>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleChildUpdateProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleChildUpdate packet, Player player)
        {
            vehicleData.UpdateVehicleChildObjects(packet.VehicleId, packet.InteractiveChildIdentifiers);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
