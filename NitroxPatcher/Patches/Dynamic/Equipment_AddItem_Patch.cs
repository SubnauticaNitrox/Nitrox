using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Equipment_AddItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Equipment t) => t.AddItem(default(string), default(InventoryItem), default(bool)));

        public static void Postfix(Equipment __instance, bool __result, string slot, InventoryItem newItem)
        {
            if (__result)
            {
                NitroxServiceLocator.LocateService<EquipmentSlots>().BroadcastEquip(newItem.item, __instance.owner, slot);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
