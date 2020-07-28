using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleChildUpdateProcessor : ClientPacketProcessor<VehicleChildUpdate>
    {
        private readonly Vehicles vehicles;

        public VehicleChildUpdateProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(VehicleChildUpdate vehicleChildUpdate)
        {
            vehicles.UpdateVehicleChildren(vehicleChildUpdate.VehicleId, vehicleChildUpdate.InteractiveChildIdentifiers);
        }
    }
}
