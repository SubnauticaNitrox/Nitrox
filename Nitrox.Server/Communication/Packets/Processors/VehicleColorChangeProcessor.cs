using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class VehicleColorChangeProcessor : AuthenticatedPacketProcessor<VehicleColorChange>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleColorChangeProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleColorChange packet, Player player)
        {
            vehicleManager.UpdateVehicleColours(packet.Index, packet.Id, packet.HSB);
            playerManager.SendPacketToOtherPlayers(packet, player);

            Log.Debug("Received packet: " + packet);
        }
    }
}
