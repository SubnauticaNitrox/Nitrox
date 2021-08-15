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
        private static readonly MethodInfo targetMethod = typeof(Bench).GetMethod("OnPlayerDeath", BindingFlags.NonPublic | BindingFlags.Instance);
        private static LocalPlayer localPlayer;
        private static SimulationOwnership simulationOwnership;

        public static void Postfix(Bench __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT);
            localPlayer.AnimationChange(AnimChangeType.BENCH, AnimChangeState.UNSET);
        }

        public override void Patch(Harmony harmony)
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
