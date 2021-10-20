using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class EnergyMixin_OnRemoveItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.OnRemoveItem(default(InventoryItem)));

        public static void Postfix(EnergyMixin __instance, InventoryItem item)
        {
            if (item != null)
            {
                // For now only broadcast, if it is a vehicle
                // Items that use batteries also have EnergyMixin components. But the items will be serialized with the battery
                // and therefore don't need to be synchronized this way
                if (__instance.gameObject.GetComponent<Vehicle>() || __instance.gameObject.GetComponentInParent<Vehicle>() || __instance.gameObject.GetComponentInParent<SubRoot>())
                {
                    NitroxServiceLocator.LocateService<StorageSlots>().BroadcastItemRemoval(__instance.gameObject);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
