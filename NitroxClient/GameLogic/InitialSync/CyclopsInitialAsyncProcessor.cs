using System.Collections;
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
        private int cyclopsLoaded = 0;
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

            totalCyclopsToLoad = packet.Vehicles.Where(v => v.TechType.Enum() == TechType.Cyclops).Count();

            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.Enum() == TechType.Cyclops)
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

            // After all cyclops are created
            if (cyclopsLoaded == totalCyclopsToLoad)
            {
                vehicles.VehicleCreated -= OnVehicleCreated;
                Log.Debug($"Spawned cyclops {NitroxEntity.GetId(gameObject)}");
            }

            Log.Debug($"We still need to load {totalCyclopsToLoad - cyclopsLoaded} cyclops");
        }
    }
}
