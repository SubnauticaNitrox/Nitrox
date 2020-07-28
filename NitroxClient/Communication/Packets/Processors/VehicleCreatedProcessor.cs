using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleCreatedProcessor : ClientPacketProcessor<VehicleCreated>
    {
        private readonly Vehicles vehicles;

        public VehicleCreatedProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(VehicleCreated packet)
        {
            vehicles.CreateVehicle(packet.CreatedVehicle);
        }
    }
}
