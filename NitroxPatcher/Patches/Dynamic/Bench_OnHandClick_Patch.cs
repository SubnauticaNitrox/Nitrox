using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bench_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Bench).GetMethod(nameof(Bench.OnHandClick), BindingFlags.Public | BindingFlags.Instance);

        private static bool skipPrefix = false;

        public static bool Prefix(Bench __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the bench/chair: {id}");
                return true;
            }

            HandInteraction<Bench> context = new HandInteraction<Bench>(__instance, hand);
            LockRequest<HandInteraction<Bench>> lockRequest = new LockRequest<HandInteraction<Bench>>(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

            simulationOwnership.RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Bench> context)
        {
            Bench bench = context.Target;

            if (lockAquired)
            {
                skipPrefix = true;
                TARGET_METHOD.Invoke(bench, new[] { context.GuiHand });
                skipPrefix = false;
            }
            else
            {
                bench.gameObject.AddComponent<DenyOwnershipHand>();
                bench.isValidHandTarget = false;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
