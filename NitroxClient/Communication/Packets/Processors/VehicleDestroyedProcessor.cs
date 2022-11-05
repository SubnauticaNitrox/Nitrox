using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleDestroyedProcessor : ClientPacketProcessor<VehicleDestroyed>
    {
        private readonly Vehicles vehicles;

        public VehicleDestroyedProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(VehicleDestroyed packet)
        {
            vehicles.DestroyVehicle(packet.Id, packet.GetPilotingMode);
        }
    }
}
