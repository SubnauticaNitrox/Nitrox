using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Bench_OnPlayerDeath_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.OnPlayerDeath(default(Player)));

    public static void Postfix(Bench __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<LocalPlayer>().BroadcastBenchChanged(id, BenchChanged.BenchChangeState.UNSET);
            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }

    }
}
