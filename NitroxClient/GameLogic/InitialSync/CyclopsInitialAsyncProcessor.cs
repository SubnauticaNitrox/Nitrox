using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class CyclopsInitialAsyncProcessor : AsyncInitialSyncProcessor
    {
        private readonly Vehicles vehicles;
        private int cyclopsStillLoading = 0;

        public CyclopsInitialAsyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(InitialPlayerSync packet)
        {
            vehicles.VehicleCreated += OnVehicleCreated;
            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.Enum() == TechType.Cyclops)
                {
                    cyclopsStillLoading++;
                    vehicles.CreateVehicle(vehicle);
                }
            }
            // If no cyclops is created, just send the finish right away
            if(cyclopsStillLoading == 0)
            {
                FinishedCreating();
            }
        }

        private void OnVehicleCreated(GameObject gameObject)
        {
            if (cyclopsStillLoading > 0)
            {
                cyclopsStillLoading--;
                // After all cyclops are created
                if (cyclopsStillLoading == 0)
                {
                    FinishedCreating();
                }
            }
        }

        private void FinishedCreating()
        {
            // Deregister event, cause we do not care about other vehicles
            vehicles.VehicleCreated -= OnVehicleCreated;
            // Fire mark completed to resume the remaining processors
            MarkCompleted();
        }
    }
}
