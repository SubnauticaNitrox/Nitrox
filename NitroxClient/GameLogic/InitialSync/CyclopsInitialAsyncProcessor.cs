using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class CyclopsInitialAsyncProcessor : InitialSyncProcessor
    {
        private readonly Vehicles vehicles;
        private int cyclopsStillLoading = 0;
        private int totalCyclopsToLoad = 0;
        private WaitScreen.ManualWaitItem waitScreenItem;

        public CyclopsInitialAsyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            this.waitScreenItem = waitScreenItem;

            vehicles.VehicleCreated += OnVehicleCreated;

            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.Enum() == TechType.Cyclops)
                {
                    cyclopsStillLoading++;
                    vehicles.CreateVehicle(vehicle);
                }
            }

            totalCyclopsToLoad = cyclopsStillLoading;

            yield return new WaitUntil(() => cyclopsStillLoading == 0);
        }

        private void OnVehicleCreated(GameObject gameObject)
        {
            waitScreenItem.SetProgress((totalCyclopsToLoad - cyclopsStillLoading), totalCyclopsToLoad);
            
            cyclopsStillLoading--;

            // After all cyclops are created
            if (cyclopsStillLoading == 0)
            {
                vehicles.VehicleCreated -= OnVehicleCreated;
            }            
        }
    }
}
