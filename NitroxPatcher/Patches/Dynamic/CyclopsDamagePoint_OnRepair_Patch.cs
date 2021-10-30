using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> would seem like the correct method to patch, but adding to its postfix will
    /// execute before <see cref="CyclopsDamagePoint.OnRepair"/> is finished working. Both owners and non-owners will be able to repair damage points on a ship.
    /// </summary>
    class CyclopsDamagePoint_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDamagePoint t) => t.OnRepair());

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
