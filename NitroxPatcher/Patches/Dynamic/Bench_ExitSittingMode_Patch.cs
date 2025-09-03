using System.Collections;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Bench_ExitSittingMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.ExitSittingMode(default, default));

    public static void Prefix(ref bool __runOriginal)
    {
        __runOriginal = !PlayerChatManager.Instance.IsChatSelected && !DevConsole.instance.selected;
    }

    public static void Postfix(Bench __instance, bool __runOriginal)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);

            Resolve<LocalPlayer>().BroadcastBenchChanged(id, BenchChanged.BenchChangeState.STANDING_UP);
            __instance.StartCoroutine(ResetAnimationDelayed(id, __instance.standUpCinematicController.interpolationTimeOut));
        }
    }

    private static IEnumerator ResetAnimationDelayed(NitroxId benchId, float delay)
    {
        yield return new WaitForSeconds(delay);
        Resolve<LocalPlayer>().BroadcastBenchChanged(benchId, BenchChanged.BenchChangeState.UNSET);
    }
}
