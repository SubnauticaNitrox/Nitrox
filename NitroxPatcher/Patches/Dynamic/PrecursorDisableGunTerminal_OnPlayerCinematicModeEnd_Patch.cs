using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs the firstUse state of the PrecursorDisableGunTerminal (the terminal at the Precursor Gun facility).
/// This ensures remote players see the correct animation (first use is longer, subsequent uses are shorter).
/// The firstUse flag is set to false in OnPlayerCinematicModeEnd after the cinematic completes.
/// </summary>
public sealed partial class PrecursorDisableGunTerminal_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDisableGunTerminal t) => t.OnPlayerCinematicModeEnd());

    public static void Postfix(PrecursorDisableGunTerminal __instance)
    {
        // After the cinematic ends, firstUse is set to false - sync this to other players
        // The terminal component is on a child object, so we need to look up the hierarchy for NitroxEntity
        if (__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Debug($"[PrecursorDisableGunTerminal] Broadcasting metadata update: firstUse={__instance.firstUse}, id={entity.Id}");
            Resolve<Entities>().BroadcastMetadataUpdate(entity.Id, new PrecursorDisableGunTerminalMetadata(__instance.firstUse));
        }
        else
        {
            Log.Warn($"[PrecursorDisableGunTerminal] No NitroxEntity found in hierarchy for {__instance.gameObject.GetFullHierarchyPath()}");
        }
    }
}
