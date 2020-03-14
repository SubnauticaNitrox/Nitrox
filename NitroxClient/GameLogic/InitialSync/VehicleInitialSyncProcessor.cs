using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
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
        
        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalSyncedVehicles = 0;
            int nonCyclopsVehicleCount = packet.Vehicles.Where(v => v.TechType.Enum() != TechType.Cyclops).Count();

            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.Enum() != TechType.Cyclops)
                {
                    waitScreenItem.SetProgress(totalSyncedVehicles, nonCyclopsVehicleCount);
                    vehicles.CreateVehicle(vehicle);
                    totalSyncedVehicles++;
                    yield return null;
                }
            }

            Log.Info("Recieved initial sync with " + totalSyncedVehicles + " non-cyclops vehicles");
        }
    }
}
