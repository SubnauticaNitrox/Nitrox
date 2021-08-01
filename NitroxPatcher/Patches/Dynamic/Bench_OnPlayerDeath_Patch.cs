﻿using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bench_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(Bench).GetMethod("OnPlayerDeath", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Bench __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, targetMethod);
        }
    }
}
