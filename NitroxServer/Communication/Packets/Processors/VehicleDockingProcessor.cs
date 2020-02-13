using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleDockingProcessor : AuthenticatedPacketProcessor<VehicleDocking>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleDockingProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleDocking packet, Player player)
        {
            Optional<VehicleModel> vehicle = vehicleData.GetVehicleModel(packet.VehicleId);
            if (!vehicle.IsPresent())
            {
                Log.Error($"VehicleDocking received for vehicle id {packet.VehicleId} that does not exist!");
                return;
            }

            VehicleModel vehicleModel = vehicle.Get();
            vehicleModel.DockingBayId = Optional<NitroxId>.Of(packet.DockId);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
