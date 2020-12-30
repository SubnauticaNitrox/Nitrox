using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PilotingChair_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PilotingChair);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPlayerDeath", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(PilotingChair __instance)
        {
            SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
            Validate.NotNull(subRoot, "PilotingChair cannot find it's corresponding SubRoot!");
            NitroxId id = NitroxEntity.GetId(subRoot.gameObject);

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, null);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
