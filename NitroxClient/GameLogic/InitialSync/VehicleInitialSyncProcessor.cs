﻿using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

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
            int nonCyclopsVehicleCount = packet.Vehicles.Count(v => v.TechType.ToUnity() != TechType.Cyclops);

            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.ToUnity() != TechType.Cyclops)
                {
                    waitScreenItem.SetProgress(totalSyncedVehicles, nonCyclopsVehicleCount);
                    vehicles.CreateVehicle(vehicle);
                    totalSyncedVehicles++;
                    yield return null;
                }
            }

            Log.Info("Received initial sync with " + totalSyncedVehicles + " non-cyclops vehicles");
        }
    }
}
