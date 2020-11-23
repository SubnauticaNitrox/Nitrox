using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(VehicleDockingBay);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(VehicleDockingBay __instance, Collider other)
        {
            Vehicle vehicle = other.GetComponentInParent<Vehicle>();
            Log.Debug($"In OnTriggerEnter Prefix. Collider: {other}, Vehicle: {vehicle}");
            return NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(NitroxEntity.GetId(vehicle.gameObject));
        }

        public static void Postfix(VehicleDockingBay __instance, Collider other)
        {
            Log.Debug("In OnTriggerEnter Postfix");
            Vehicle interpolatingVehicle = (Vehicle)__instance.ReflectionGet("interpolatingVehicle");
            Log.Debug($"interpolatingVehicle: {interpolatingVehicle}");
            if (interpolatingVehicle && NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(NitroxEntity.GetId(interpolatingVehicle.gameObject)))
            {
                Log.Debug("Will send vehicle docking");

                NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingVehicle);
            }
        }
        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
