using System;
using System.Collections.ObjectModel;
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
            // Below code is adapted from CrafterLogic.TryPickupAsync

            // Certain crafts give you multiple items, so we need to know how many are left
            // or in case there are none left, we send TechType.None
            int amountLeft = Math.Max(__instance.numCrafted - 1, 0); // Logic below 0 would break
            int linkedIndex = __instance.linkedIndex;

            ReadOnlyCollection<TechType> linkedItems = TechData.GetLinkedItems(__instance.craftingTechType);
            int linkedItemsCount = linkedItems?.Count ?? 0;

            TechType techType = __instance.craftingTechType;

            // Sometimes, there are linked items appearing after the end of the regular craft results
            // We implement this logic now to get the real values because it normally happens after this prefix
            if (amountLeft == 0)
            {
                linkedIndex++;
                if (linkedIndex < linkedItemsCount)
                {
                    amountLeft = 1;
                }
                else
                {
                    techType = TechType.None;
                }
            }

            Resolve<Entities>().BroadcastMetadataUpdate(crafterId, new CrafterMetadata(techType.ToDto(), __instance.timeCraftingBegin, 0f, amountLeft, linkedIndex));
        }

        // The Pickup() item codepath will inform the server that the item was added to the inventory.
    }
}
