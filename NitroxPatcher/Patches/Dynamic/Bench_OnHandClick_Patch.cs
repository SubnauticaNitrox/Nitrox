using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bench_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.OnHandClick(default(GUIHand)));
        private static LocalPlayer localPlayer;
        private static SimulationOwnership simulationOwnership;
        private static bool skipPrefix;

        public static bool Prefix(Bench __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }

            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the bench/chair: {id}");
                return true;
            }

            HandInteraction<Bench> context = new(__instance, hand);
            LockRequest<HandInteraction<Bench>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

            simulationOwnership.RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Bench> context)
        {
            Bench bench = context.Target;

            if (lockAquired)
            {
                skipPrefix = true;
                bench.OnHandClick(context.GuiHand);
                localPlayer.AnimationChange(AnimChangeType.BENCH, AnimChangeState.ON);
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
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
