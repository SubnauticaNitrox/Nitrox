using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(VehicleDockingBay);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        private static Vehicle prevInterpolatingVehicle = null;

        public static bool Prefix(VehicleDockingBay __instance, Collider other)
        {
            Vehicle vehicle = other.GetComponentInParent<Vehicle>();
            prevInterpolatingVehicle = (Vehicle)__instance.ReflectionGet("interpolatingVehicle");
            return NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(NitroxEntity.GetId(vehicle.gameObject));
        }

        public static void Postfix(VehicleDockingBay __instance, Collider other)
        {
            Vehicle interpolatingVehicle = (Vehicle)__instance.ReflectionGet("interpolatingVehicle");
            NitroxId id = NitroxEntity.GetId(interpolatingVehicle.gameObject);
            // Only send data, when interpolatingVehicle changes to avoid multiple packages send
            if (interpolatingVehicle &&
                interpolatingVehicle != prevInterpolatingVehicle &&
                NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(id))
            {
                Log.Debug($"Will send vehicle docking for {id}");
                NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingVehicle);
            }
        }
        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
