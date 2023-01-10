using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class GhostCrafter_OnCraftingEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GhostCrafter t) => t.OnCraftingEnd());

    public static bool Prefix(GhostCrafter __instance)
    {
        NitroxId id = NitroxEntity.GetId(__instance.gameObject);

        // The OnCraftingEnd patch is executed when crafting is complete and the item is about to be automatically
        // picked up by the nearest player.  We don't want all players to attempt to pick up the item.  Instead,
        // the crafting player will intiate a lock request when pushing the craft button - OnCraftingStart().
        return Resolve<SimulationOwnership>().HasAnyLockType(id);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}

