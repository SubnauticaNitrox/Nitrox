using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Bench_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.OnHandClick(default(GUIHand)));
    private static bool skipPrefix;

    public static bool Prefix(Bench __instance, GUIHand hand)
    {
        if (skipPrefix)
        {
            return true;
        }

        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"Already have an exclusive lock on the bench/chair: {id}");
            return true;
        }

        HandInteraction<Bench> context = new(__instance, hand);
        LockRequest<HandInteraction<Bench>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, HandInteraction<Bench> context)
    {
        Bench bench = context.Target;

        if (lockAcquired)
        {
            skipPrefix = true;
            bench.OnHandClick(context.GuiHand);
            Resolve<LocalPlayer>().AnimationChange(AnimChangeType.BENCH, AnimChangeState.ON);
            skipPrefix = false;
        }
        else
        {
            bench.gameObject.AddComponent<DenyOwnershipHand>();
            bench.isValidHandTarget = false;
        }
    }
}
