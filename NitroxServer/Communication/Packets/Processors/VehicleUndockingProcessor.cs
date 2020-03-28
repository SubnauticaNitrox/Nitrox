using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public VehicleUndockingProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehicleUndocking packet, Player player)
        {
            Optional<VehicleModel> vehicle = vehicleManager.GetVehicleModel(packet.VehicleId);

            if (!vehicle.HasValue)
            {
                return;
            }

            VehicleModel vehicleModel = vehicle.Value;
            vehicleModel.DockingBayId = Optional.Empty;

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
