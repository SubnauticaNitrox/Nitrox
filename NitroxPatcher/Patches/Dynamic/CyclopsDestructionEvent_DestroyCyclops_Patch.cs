using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the cyclops destruction by calling <see cref="LiveMixin.Kill"/>, and safely remove every player from it.
/// </summary>
public sealed partial class CyclopsDestructionEvent_DestroyCyclops_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDestructionEvent t) => t.DestroyCyclops());

    public static void Prefix(CyclopsDestructionEvent __instance)
    {
        __instance.subLiveMixin.Kill();
        if (__instance.TryGetNitroxId(out NitroxId nitroxId))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(nitroxId);
        }

        // Before the cyclops destruction, we move out the remote players so that they aren't stuck in its hierarchy
        if (Player.main && Player.main.currentSub == __instance.subRoot && __instance.subRoot.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.RemoveAllPlayers();
        }
    }
}
