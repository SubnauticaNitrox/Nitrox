using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class EnergyMixin_OnAddItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.OnAddItem(default(InventoryItem)));

        public static void Postfix(EnergyMixin __instance, InventoryItem item)
        {
            if (item != null)
            {
                //For now only broadcast, if it is a vehicle
                if (__instance.gameObject.GetComponent<Vehicle>() || __instance.gameObject.GetComponentInParent<Vehicle>() || __instance.gameObject.GetComponentInParent<SubRoot>())
                {
                    NitroxServiceLocator.LocateService<StorageSlots>().BroadcastItemAdd(item, __instance.gameObject);
                }

            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
