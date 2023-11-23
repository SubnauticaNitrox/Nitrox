using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Bench_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.OnPlayerDeath(default(Player)));

    public static void Postfix(Bench __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }

        Resolve<LocalPlayer>().AnimationChange(AnimChangeType.BENCH, AnimChangeState.UNSET);
    }
}
