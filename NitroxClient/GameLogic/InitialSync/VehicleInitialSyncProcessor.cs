using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class VehicleInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Vehicles vehicles;

        public VehicleInitialSyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;

            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
        }

        public override void Process(InitialPlayerSync packet)
        {
            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                // TODO: create an AsyncInitialSyncProcessor that creates cyclops before seamoth and exosuit (as seamoth can be docked in cyclops)
                vehicles.CreateVehicle(vehicle);
            }
            foreach(ExosuitModel exosuit in packet.Exosuits)
            {
                vehicles.CreateVehicle(exosuit);
            }
        }
    }
}
