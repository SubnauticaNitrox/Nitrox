using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class QuickSlots_Bind_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((QuickSlots t) => t.Bind(default(int), default(InventoryItem)));

    public static void Postfix(QuickSlots __instance)
    {
        NitroxId[] slotItemIds = new NitroxId[__instance.binding.Length];

        for (int i = 0; i < __instance.binding.Length; i++)
        {
            InventoryItem inventoryItem = __instance.binding[i];

            if (inventoryItem != null && inventoryItem.item)
            {
                slotItemIds[i] = NitroxEntity.GetId(inventoryItem.item.gameObject);
            }
        }

        Resolve<LocalPlayer>().BroadcastQuickSlotsBindingChanged(slotItemIds);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
