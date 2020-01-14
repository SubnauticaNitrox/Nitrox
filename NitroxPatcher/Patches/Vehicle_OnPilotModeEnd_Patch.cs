﻿using System;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    public class Vehicle_OnPilotModeEnd_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPilotModeEnd", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastOnPilotModeChanged(__instance, false);

            NitroxId id = NitroxIdentifier.GetId(__instance.gameObject);
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, null);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
