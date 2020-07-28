using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleDestroyedProcessor : ClientPacketProcessor<VehicleDestroyed>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleDestroyedProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleDestroyed packet)
        {
            using (packetSender.Suppress<VehicleDestroyed>())
            {
                vehicles.DestroyVehicle(packet.Id, packet.GetPilotingMode);
            }
        }
    }
}
