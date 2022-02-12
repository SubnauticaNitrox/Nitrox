using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ItemsContainer_NotifyAddItem_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ItemsContainer t) => t.NotifyAddItem(default(InventoryItem)));

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (item != null)
            {
                NitroxServiceLocator.LocateService<ItemContainers>().BroadcastItemAdd(item.item, __instance.tr);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
