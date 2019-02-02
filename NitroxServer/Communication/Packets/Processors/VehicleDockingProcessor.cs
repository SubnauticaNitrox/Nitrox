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
            Optional<VehicleModel> vehicle = vehicleData.GetVehicleModel(packet.VehicleGuid);
            if (!vehicle.IsPresent())
            {
                Log.Error("VehicleDocking received for vehicle guid {0} that does not exist!", packet.VehicleGuid);
                return;
            }

            VehicleModel vehicleModel = vehicle.Get();
            vehicleModel.DockingBayGuid = Optional<string>.Of(packet.DockGuid);

            // We don't need to send this packet to the other clients, as those clients
            // will automatically dock the vehicle when it enters the trigger
        }
    }
}
