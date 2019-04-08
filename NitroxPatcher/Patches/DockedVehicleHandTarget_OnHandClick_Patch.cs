﻿using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using System;
using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    public class DockedVehicleHandTarget_OnHandClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DockedVehicleHandTarget);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        private static DockedVehicleHandTarget dockedVehicle;
        private static VehicleDockingBay vehicleDockingBay;
        private static GUIHand guiHand;
        private static bool skipPrefix = false;

        public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
        {
            vehicleDockingBay = __instance.dockingBay;
            Vehicle vehicle = vehicleDockingBay.GetDockedVehicle();

            if (skipPrefix || vehicle == null)
            {
                return true;
            }
            
            dockedVehicle = __instance;
            guiHand = hand;

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            NitroxId id = NitroxIdentifier.GetId(vehicle.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on this vehicle: {id}");
                return true;
            }

            simulationOwnership.RequestSimulationLock(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired)
        {
            if (lockAquired)
            {
                NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleUndocking(vehicleDockingBay, vehicleDockingBay.GetDockedVehicle());

                skipPrefix = true;
                TARGET_METHOD.Invoke(dockedVehicle, new[] { guiHand });
                skipPrefix = false;
            }
            else
            {
                HandReticle.main.SetInteractText("Another player is using this vehicle!");
                HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                dockedVehicle.isValidHandTarget = false;
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
