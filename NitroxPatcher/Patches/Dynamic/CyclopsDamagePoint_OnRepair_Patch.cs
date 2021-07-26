using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> would seem like the correct method to patch, but adding to its postfix will
    /// execute before <see cref="CyclopsDamagePoint.OnRepair"/> is finished working. Both owners and non-owners will be able to repair damage points on a ship.
    /// </summary>
    class CyclopsDamagePoint_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsDamagePoint);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsDamagePoint __instance)
        {
            // If the amount is high enough, it'll heal full
            NitroxServiceLocator.LocateService<Cyclops>().OnDamagePointRepaired(__instance.GetComponentInParent<SubRoot>(), __instance, 999);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
