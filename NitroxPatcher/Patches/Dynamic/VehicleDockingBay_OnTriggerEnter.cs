using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((VehicleDockingBay t) => t.OnTriggerEnter(default(Collider)));
        private static Vehicle prevInterpolatingVehicle;

        public static bool Prefix(VehicleDockingBay __instance, Collider other)
        {
            Vehicle vehicle = other.GetComponentInParent<Vehicle>();
            prevInterpolatingVehicle = __instance.interpolatingVehicle;
            Optional<NitroxId> opVehicleId = vehicle.GetId();
            return !vehicle || (opVehicleId.HasValue && Resolve<SimulationOwnership>().HasAnyLockType(opVehicleId.Value));
        }

        public static void Postfix(VehicleDockingBay __instance)
        {
            Vehicle interpolatingVehicle = __instance.interpolatingVehicle;
            // Only send data, when interpolatingVehicle changes to avoid multiple packages send
            if (!interpolatingVehicle || interpolatingVehicle == prevInterpolatingVehicle)
            {
                return;
            }

            if (interpolatingVehicle.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
            {
                Log.Debug($"Will send vehicle docking for {id}");
                Resolve<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingVehicle);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
