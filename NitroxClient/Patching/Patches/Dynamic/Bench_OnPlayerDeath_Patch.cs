using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class Bench_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
{
    private static LocalPlayer localPlayer;
    private static SimulationOwnership simulationOwnership;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.OnPlayerDeath(default(Player)));

    public Bench_OnPlayerDeath_Patch(LocalPlayer lp, SimulationOwnership so)
    {
        localPlayer = lp ?? throw new ArgumentNullException(nameof(lp));
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
    }

    public static void Postfix(Bench __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            localPlayer.BroadcastBenchChanged(id, BenchChanged.BenchChangeState.UNSET);
            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }
    }
}
