using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class CyclopsInitialAsyncProcessor : InitialSyncProcessor
    {
        private readonly Vehicles vehicles;
        private WaitScreen.ManualWaitItem waitScreenItem;
        private int cyclopsLoaded = 0, totalCyclopsToLoad = 0;

        public CyclopsInitialAsyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            IEnumerable<VehicleModel> cyclops = packet.Vehicles.Where(v => v.TechType.Enum() == TechType.Cyclops);
            totalCyclopsToLoad = cyclops.Count();

            this.waitScreenItem = waitScreenItem;

            if (totalCyclopsToLoad > 0)
            {
                vehicles.VehicleCreated += OnVehicleCreated;

                foreach (VehicleModel vehicle in cyclops)
                {
                    Log.Debug($"Trying to spawn {vehicle}");
                    vehicles.CreateVehicle(vehicle);
                }
            }

            yield return new WaitUntil(() => cyclopsLoaded == totalCyclopsToLoad);
        }

        private void OnVehicleCreated(GameObject gameObject)
        {
            cyclopsLoaded++;
            waitScreenItem.SetProgress(cyclopsLoaded, totalCyclopsToLoad);

            Log.Debug($"Spawned cyclops {NitroxEntity.GetId(gameObject)}");

            // After all cyclops are created
            if (cyclopsLoaded == totalCyclopsToLoad)
            {
                vehicles.VehicleCreated -= OnVehicleCreated;
            }
            else
            {
                Log.Debug($"We still need to load {totalCyclopsToLoad - cyclopsLoaded} cyclops");
            }
        }
    }
}
