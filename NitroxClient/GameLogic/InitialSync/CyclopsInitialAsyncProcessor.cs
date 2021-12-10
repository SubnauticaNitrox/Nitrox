using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class CyclopsInitialAsyncProcessor : InitialSyncProcessor
    {
        private int cyclopsLoaded;
        private int totalCyclopsToLoad;
        private readonly Vehicles vehicles;
        private WaitScreen.ManualWaitItem waitScreenItem;

        public CyclopsInitialAsyncProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            IList<VehicleModel> cyclopses = packet.Vehicles.Where(v => v.TechType.ToUnity() == TechType.Cyclops).ToList();
            totalCyclopsToLoad = cyclopses.Count;

            this.waitScreenItem = waitScreenItem;

            if (totalCyclopsToLoad > 0)
            {
                vehicles.VehicleCreated += OnVehicleCreated;

                foreach (VehicleModel vehicle in cyclopses)
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
