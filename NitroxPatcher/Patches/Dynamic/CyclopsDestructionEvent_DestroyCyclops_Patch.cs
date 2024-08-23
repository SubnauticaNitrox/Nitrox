using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
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
        if (__instance.TryGetNitroxId(out NitroxId nitroxId))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(nitroxId);
            EntityPositionBroadcaster.StopWatchingEntity(nitroxId);
        }

        bool wasInCyclops = Player.main.currentSub == __instance.subRoot;

        // Before the cyclops destruction, we move out the remote players so that they aren't stuck in its hierarchy
        if (__instance.subRoot && __instance.subRoot.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.RemoveAllPlayers();
        }

        if (wasInCyclops)
        {
            // Particular case here, this is not broadcasted and should not be, it's just there to have player be really inside the cyclops while not being registered by NitroxCyclops
            Player.main._currentSub = __instance.subRoot;
        }

        __instance.subLiveMixin.Kill();
    }
}
