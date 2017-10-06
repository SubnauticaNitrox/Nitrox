using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class Equipment_AddItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Equipment);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AddItem", BindingFlags.Public | BindingFlags.Instance);
        
        public static void Postfix(Equipment __instance, bool __result, string slot, InventoryItem newItem)
        {
            if(__result)
            {
                Multiplayer.Logic.EquipmentSlots.Equip(newItem.item, __instance.owner, slot);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
