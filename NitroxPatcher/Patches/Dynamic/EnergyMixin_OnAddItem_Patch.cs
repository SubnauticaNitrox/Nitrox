using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class EnergyMixin_OnAddItem_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.OnAddItem(default(InventoryItem)));

    public static void Postfix(EnergyMixin __instance, InventoryItem item)
    {
        if (item != null)
        {
            Resolve<ItemContainers>().BroadcastBatteryAdd(item.item.gameObject, __instance.gameObject, item.techType);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
