using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehiclePositionFixProcessor : AuthenticatedPacketProcessor<VehiclePositionFix>
    {
        private readonly VehicleManager vehicleManager;

        public VehiclePositionFixProcessor(VehicleManager vehicleManager)
        {
            this.vehicleManager = vehicleManager;
        }

        public override void Process(VehiclePositionFix packet, Player player)
        {
            Log.Info(packet);
            vehicleManager.UpdateVehiclePosition(packet.VehicleId, packet.Position);
        }
    }
}
