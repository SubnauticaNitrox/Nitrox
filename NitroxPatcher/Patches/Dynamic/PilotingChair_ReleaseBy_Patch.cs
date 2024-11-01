using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PilotingChair_ReleaseBy_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PilotingChair t) => t.ReleaseBy(default(Player)));

    public static void Postfix(PilotingChair __instance)
    {
        SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
        Validate.NotNull(subRoot, "PilotingChair cannot find it's corresponding SubRoot!");

        if (subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
            Resolve<Vehicles>().BroadcastOnPilotModeChanged(__instance.subRoot.gameObject, false);
        }
    }
}
