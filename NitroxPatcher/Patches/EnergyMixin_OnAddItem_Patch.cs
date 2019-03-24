using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    class EnergyMixin_OnAddItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EnergyMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnAddItem", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(EnergyMixin __instance, InventoryItem item)
        {
            if (item != null)
            {
                //For now only broadcast, if it is a vehicle
                GameObject gameObject = __instance.gameObject;
                if (gameObject.GetComponent<Vehicle>() != null || gameObject.GetComponentInParent<Vehicle>() != null || gameObject.GetComponentInParent<SubRoot>() != null)
                {
                    NitroxServiceLocator.LocateService<StorageSlots>().BroadcastItemAdd(item, __instance.gameObject);
                }               
                
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
