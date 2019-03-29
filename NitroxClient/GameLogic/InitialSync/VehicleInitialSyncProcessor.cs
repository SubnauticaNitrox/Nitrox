using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.GameLogic.InitialSync
{
    public class VehicleInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Vehicles vehicles;

        public VehicleInitialSyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;

            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
        }
        
        public override void Process(InitialPlayerSync packet)
        { 
            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.Enum() != TechType.Cyclops)
                {
                    vehicles.CreateVehicle(vehicle);
                }
            }
        }
    }
}
