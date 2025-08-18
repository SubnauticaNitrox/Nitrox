using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the completion of a craft.
/// </summary>
public sealed partial class CrafterLogic_NotifyPickup_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrafterLogic t) => t.NotifyPickup(default));

    public static void Prefix(CrafterLogic __instance)
    {
        // See GhostCrafter_OnCraftingBegin_Patch.Postfix to know why we get NitroxId on CrafterLogic
        if (__instance.TryGetIdOrWarn(out NitroxId crafterId))
        {
            // Certain crafts give you multiple items, so we need to know how many are lefts
            // or in case there are none left, we send TechType.None
            int amountLeft = __instance.numCrafted - 1;
            TechType techType = amountLeft > 0 ? __instance.craftingTechType : TechType.None;
            Resolve<Entities>().BroadcastMetadataUpdate(crafterId, new CrafterMetadata(techType.ToDto(), amountLeft, DayNightCycle.main.timePassedAsFloat, 0f));
        }

        // The Pickup() item codepath will inform the server that the item was added to the inventory.
    }
}
