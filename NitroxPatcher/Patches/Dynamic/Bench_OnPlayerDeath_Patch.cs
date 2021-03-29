using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bench_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Bench);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPlayerDeath", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Bench __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
