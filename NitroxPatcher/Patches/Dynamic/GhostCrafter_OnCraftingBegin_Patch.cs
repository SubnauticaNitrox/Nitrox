using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts crafting start and claims exclusive ownership on the crafter
/// </summary>
public sealed partial class GhostCrafter_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GhostCrafter t) => t.OnCraftingBegin(default(TechType), default(float)));

    public static void Postfix(GhostCrafter __instance, TechType techType, float duration)
    {
        // We favor targeting the CrafterLogic instead of the GhostCrafter because in the base upgrade console module, the NitroxEntity is
        // on the CrafterLogic only. On every other crafter type, both CrafterLogic and GhostCrafter are on the same object.

        // Also for base upgrade console module, crafterLogic is nullified and never updated, so we use _logic instead for every crafter
        if (__instance._logic.TryGetIdOrWarn(out NitroxId crafterLogicId))
        {
            Resolve<Entities>().BroadcastMetadataUpdate(crafterLogicId, new CrafterMetadata(techType.ToDto(), DayNightCycle.main.timePassedAsFloat, duration, __instance._logic.numCrafted, __instance._logic.linkedIndex));

            // Async request to be the person to auto-pickup the result. In the future this can be improved to lock down all crafting based on ownership
            // but will require redoing our hooks.
            Resolve<SimulationOwnership>().RequestSimulationLock(crafterLogicId, SimulationLockType.EXCLUSIVE);
        }
    }
}
