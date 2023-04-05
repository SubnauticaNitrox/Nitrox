using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EnergyMixin_ModifyCharge_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.ModifyCharge(default(float)));

        public static void Postfix(EnergyMixin __instance, float __result)
        {
            GameObject battery = __instance.GetBatteryGameObject();
            if (!battery)
            {
                return;
            }

            if (Math.Abs(Math.Floor(__instance.charge) - Math.Floor(__instance.charge - __result)) > 0.0) //Send package if power changed to next natural number
            {
                if (!NitroxEntity.TryGetIdFrom(__instance.gameObject, out NitroxId instanceId))
                {
                    Log.Warn($"[EnergyMixin_ModifyCharge_Patch] Couldn't find an instance id on {__instance.gameObject.GetFullHierarchyPath()}");
                    return;
                }

                if (!NitroxEntity.TryGetIdFrom(battery, out NitroxId batteryId))
                {
                    Log.Warn($"[EnergyMixin_ModifyCharge_Patch] Couldn't find a battery id on {battery.GetFullHierarchyPath()}");
                    return;
                }

                BasicItemData batteryData = new(instanceId, batteryId, SerializationHelper.GetBytes(battery));
                NitroxServiceLocator.LocateService<StorageSlots>().EnergyMixinValueChanged(instanceId, __instance.charge, batteryData);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
