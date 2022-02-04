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
            GameObject battery = __instance.GetBattery();
            if (battery)
            {
                if (Math.Abs(Math.Floor(__instance.charge) - Math.Floor(__instance.charge - __result)) > 0.0) //Send package if power changed to next natural number
                {
                    NitroxId instanceId = NitroxEntity.GetId(__instance.gameObject);
                    ItemData batteryData = new(instanceId, NitroxEntity.GetId(battery), SerializationHelper.GetBytes(battery));

                    NitroxServiceLocator.LocateService<StorageSlots>().EnergyMixinValueChanged(instanceId, __instance.charge, batteryData);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
