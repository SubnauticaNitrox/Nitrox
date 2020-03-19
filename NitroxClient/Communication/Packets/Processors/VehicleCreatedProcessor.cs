using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleCreatedProcessor : ClientPacketProcessor<VehicleCreated>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleCreatedProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleCreated packet)
        {
            vehicles.CreateVehicle(packet.CreatedVehicle);
        }
    }
}
