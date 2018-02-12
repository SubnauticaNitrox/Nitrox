using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class ItemsContainer_NotifyRemoveItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ItemsContainer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyRemoveItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(InventoryItem) }, null);

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if(item != null && __instance.tr.parent.name != "EscapePod" && __instance.tr.parent.name != "Player")
            {
                NitroxServiceLocator.LocateService<ItemContainers>().RemoveItem(item.item, __instance.tr.parent.gameObject);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
