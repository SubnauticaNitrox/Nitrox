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
        private readonly PlayerManager remotePlayerManager;

        public VehicleDestroyedProcessor(IPacketSender packetSender, Vehicles vehicles, PlayerManager remotePlayerManager)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.remotePlayerManager = remotePlayerManager;
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
