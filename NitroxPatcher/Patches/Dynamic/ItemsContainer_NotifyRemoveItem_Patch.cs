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
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyRemoveItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(InventoryItem) }, null);

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (item != null)
            {
                NitroxServiceLocator.LocateService<ItemContainers>().BroadcastItemRemoval(item.item, __instance.tr);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
