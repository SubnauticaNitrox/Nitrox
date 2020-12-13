using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
