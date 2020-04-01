using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ItemsContainer_NotifyRemoveItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ItemsContainer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyRemoveItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(InventoryItem) }, null);
        private static ItemContainers itemContainers = null;

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (itemContainers == null)
            {
                itemContainers = NitroxServiceLocator.LocateService<ItemContainers>();
            }
            
            if (item != null)
            {
                itemContainers.BroadcastItemRemoval(item.item, __instance.tr);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
