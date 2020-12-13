using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class VehicleChildUpdateProcessor : ClientPacketProcessor<VehicleChildUpdate>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly Vehicles vehicles;

        public VehicleChildUpdateProcessor(PlayerManager remotePlayerManager, Vehicles vehicles)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleChildUpdate vehicleChildUpdate)
        {
            vehicles.UpdateVehicleChildren(vehicleChildUpdate.VehicleId, vehicleChildUpdate.InteractiveChildIdentifiers);
        }
    }
}
