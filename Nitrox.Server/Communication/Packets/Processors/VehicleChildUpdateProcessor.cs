using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class VehicleChildUpdateProcessor : AuthenticatedPacketProcessor<VehicleChildUpdate>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleChildUpdateProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleChildUpdate packet, Player player)
        {
            vehicleManager.UpdateVehicleChildObjects(packet.VehicleId, packet.InteractiveChildIdentifiers);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
