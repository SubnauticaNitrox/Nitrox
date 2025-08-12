using System;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EnergyMixin_ModifyCharge_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.ModifyCharge(default(float)));

    public static void Postfix(EnergyMixin __instance, float __result)
    {
        GameObject batteryGo = __instance.GetBatteryGameObject();

        if (batteryGo && batteryGo.TryGetComponent(out Battery battery) &&
            Math.Abs(Math.Floor(__instance.charge) - Math.Floor(__instance.charge - __result)) > 0.0 && //Send package if power changed to next natural number
            batteryGo.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(battery, id);
        }
    }
}
