using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> would seem like the correct method to patch, but adding to its postfix will
    /// execute before <see cref="CyclopsDamagePoint.OnRepair"/> is finished working.
    /// </summary>
    class CyclopsDamagePoint_OnRepair_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsDamagePoint);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsDamagePoint __instance)
        {
            Multiplayer.Logic.Cyclops.ExternalDamagePointRepaired(__instance.GetComponentInParent<SubRoot>());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
