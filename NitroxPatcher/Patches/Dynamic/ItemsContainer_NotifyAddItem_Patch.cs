using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ItemsContainer_NotifyAddItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ItemsContainer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyAddItem", BindingFlags.NonPublic | BindingFlags.Instance);
        private static ItemContainers itemContainers;

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (itemContainers == null)
            {
                itemContainers = NitroxServiceLocator.LocateService<ItemContainers>();
            }
            
            if (item != null)
            {
                itemContainers.BroadcastItemAdd(item.item, __instance.tr);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
