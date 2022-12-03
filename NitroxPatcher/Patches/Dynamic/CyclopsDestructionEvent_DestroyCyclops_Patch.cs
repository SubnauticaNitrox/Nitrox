using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class CyclopsDestructionEvent_DestroyCyclops_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDestructionEvent t) => t.DestroyCyclops());

    public static void Prefix(CyclopsDestructionEvent __instance)
    {
        __instance.subLiveMixin.Kill();
        if (NitroxEntity.TryGetEntityFrom(__instance.gameObject, out NitroxEntity nitroxEntity))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(nitroxEntity.Id);
        }

        // Before the cyclops destruction, we move out the remote players so that they aren't stuck in its hierarchy
        foreach (RemotePlayerIdentifier remotePlayerIdentifier in __instance.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            remotePlayerIdentifier.RemotePlayer.ResetStates();
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
