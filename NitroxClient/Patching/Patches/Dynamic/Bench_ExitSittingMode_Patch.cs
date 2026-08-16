using System;
using System.Collections;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using UnityEngine;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class Bench_ExitSittingMode_Patch : NitroxPatch, IDynamicPatch
{
    private static LocalPlayer localPlayer;
    private static SimulationOwnership simulationOwnership;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.ExitSittingMode(default, default));

    public Bench_ExitSittingMode_Patch(LocalPlayer lp, SimulationOwnership so)
    {
        localPlayer = lp ?? throw new ArgumentNullException(nameof(lp));
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
    }

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
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT);

            localPlayer.BroadcastBenchChanged(id, BenchChanged.BenchChangeState.STANDING_UP);
            __instance.StartCoroutine(ResetAnimationDelayed(id, __instance.standUpCinematicController.interpolationTimeOut));
        }
    }

    private static IEnumerator ResetAnimationDelayed(NitroxId benchId, float delay)
    {
        yield return new WaitForSeconds(delay);
        localPlayer.BroadcastBenchChanged(benchId, BenchChanged.BenchChangeState.UNSET);
    }
}
