using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.OnPlayerCinematicModeEnd());

    public static void Postfix(BulkheadDoor __instance)
    {
        Log.Info("[BulkheadDoor_OnPlayerCinematicModeEnd_Patch] Postfix called");

        if (!__instance.TryGetComponentInParent<NitroxEntity>(out NitroxEntity nitroxEntity, true))
        {
            Log.Info("[BulkheadDoor_OnPlayerCinematicModeEnd_Patch] Could not find NitroxEntity in parent hierarchy");
            return;
        }

        NitroxId id = nitroxEntity.Id;

        Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
    }
}
