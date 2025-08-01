using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Equipment_RemoveItem_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Equipment t) => t.RemoveItem(default, default, default));

    public static void Postfix(Equipment __instance, InventoryItem __result)
    {
        if (__result != null)
        {
            Resolve<EquipmentSlots>().BroadcastUnequip(__result.item, __instance.owner);
        }
    }
}
