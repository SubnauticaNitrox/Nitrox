using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class ItemsContainer_NotifyAddItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ItemsContainer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyAddItem", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (item != null)
            {
                NitroxServiceLocator.LocateService<ItemContainers>().BroadcastItemAdd(item.item, __instance.tr);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
