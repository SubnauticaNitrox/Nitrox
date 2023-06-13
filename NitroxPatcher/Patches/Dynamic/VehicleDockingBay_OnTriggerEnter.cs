using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((VehicleDockingBay t) => t.OnTriggerEnter(default(Collider)));
#if SUBNAUTICA
        private static Vehicle prevInterpolatingVehicle;
#elif BELOWZERO
        private static Dockable prevInterpolatingDockable;
#endif

        public static bool Prefix(VehicleDockingBay __instance, Collider other)
        {
            Vehicle vehicle = other.GetComponentInParent<Vehicle>();
#if SUBNAUTICA
            prevInterpolatingVehicle = __instance.interpolatingVehicle;
#elif BELOWZERO
            prevInterpolatingDockable = __instance.interpolatingDockable;
#endif
            return vehicle == null || Resolve<SimulationOwnership>().HasAnyLockType(NitroxEntity.GetId(vehicle.gameObject));
        }

        public static void Postfix(VehicleDockingBay __instance)
        {
#if SUBNAUTICA
            Vehicle interpolatingVehicle = __instance.interpolatingVehicle;
            // Only send data, when interpolatingVehicle changes to avoid multiple packages send
            if (!interpolatingVehicle || interpolatingVehicle == prevInterpolatingVehicle)
            {
                return;
            }
            NitroxId id = NitroxEntity.GetId(interpolatingVehicle.gameObject);
            if (Resolve<SimulationOwnership>().HasAnyLockType(id))
            {
                Log.Debug($"Will send vehicle docking for {id}");
                Resolve<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingVehicle);
            }
#elif BELOWZERO
            Dockable interpolatingDockable = __instance.interpolatingDockable;
            // Only send data, when interpolatingVehicle changes to avoid multiple packages send
            if (!interpolatingDockable || interpolatingDockable == prevInterpolatingDockable)
            {
                return;
            }
            NitroxId id = NitroxEntity.GetId(interpolatingDockable.gameObject);
            if (Resolve<SimulationOwnership>().HasAnyLockType(id))
            {
                Log.Debug($"Will send vehicle docking for {id}");
                Resolve<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingDockable.vehicle);
            }
#endif
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
