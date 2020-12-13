using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : ClientPacketProcessor<VehicleMovement>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly Vehicles vehicles;

        public VehicleMovementProcessor(PlayerManager remotePlayerManager, Vehicles vehicles)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            VehicleMovementData vehicleModel = vehicleMovement.VehicleMovementData;
            Optional<RemotePlayer> player = remotePlayerManager.Find(vehicleMovement.PlayerId);
            vehicles.UpdateVehiclePosition(vehicleModel, player);
        }
    }
}
