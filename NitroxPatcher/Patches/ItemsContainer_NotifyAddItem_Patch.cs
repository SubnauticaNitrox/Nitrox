using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class ItemsContainer_NotifyAddItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ItemsContainer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyAddItem", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if(item != null && __instance.tr.parent.name != "EscapePod" && __instance.tr.parent.name != "Player")
            {
                Multiplayer.Logic.ItemContainers.AddItem(item.item, __instance.tr.parent.gameObject);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
