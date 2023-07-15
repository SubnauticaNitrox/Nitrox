using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class QuickSlots_Bind_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((QuickSlots t) => t.Bind(default(int), default(InventoryItem)));

    public static void Postfix(QuickSlots __instance)
    {
        Optional<NitroxId>[] slotItemIds = new Optional<NitroxId>[__instance.binding.Length];

        for (int i = 0; i < __instance.binding.Length; i++)
        {
            InventoryItem inventoryItem = __instance.binding[i];

            if (inventoryItem != null && inventoryItem.item)
            {
                slotItemIds[i] = inventoryItem.item.GetId();
            }
            else
            {
                slotItemIds[i] = Optional.Empty;
            }
        }

        Resolve<LocalPlayer>().BroadcastQuickSlotsBindingChanged(slotItemIds);
    }
}
