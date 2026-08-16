using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class QuickSlots_Bind_Patch : NitroxPatch, IDynamicPatch
{
    private static LocalPlayer localPlayer;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((QuickSlots t) => t.Bind(default(int), default(InventoryItem)));

    public QuickSlots_Bind_Patch(LocalPlayer lp)
    {
        localPlayer = lp;
    }

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

        localPlayer.BroadcastQuickSlotsBindingChanged(slotItemIds);
    }
}
