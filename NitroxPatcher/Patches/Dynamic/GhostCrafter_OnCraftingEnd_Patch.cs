using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class GhostCrafter_OnCraftingEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GhostCrafter t) => t.OnCraftingEnd());

    public static bool Prefix(GhostCrafter __instance)
    {
        // The OnCraftingEnd patch is executed when crafting is complete and the item is about to be automatically
        // picked up by the nearest player.  We don't want all players to attempt to pick up the item.  Instead,
        // the crafting player will intiate a lock request when pushing the craft button - OnCraftingStart().
        if (__instance.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            // once an item is crafted, we no longer require an exclusive lock.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);

            return true;
        }

        return false;
    }
}
