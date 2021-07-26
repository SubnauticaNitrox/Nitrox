using System;
using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic
{
    class EnergyMixin_SpawnDefault_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EnergyMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SpawnDefault", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(EnergyMixin __instance)
        {
            //Try to figure out if the default battery is spawned from a vehicle or cyclops
            if (__instance.gameObject.GetComponent<Vehicle>())
            {
                return false;
            }
            if (__instance.gameObject.GetComponentInParent<Vehicle>())
            {
                return false;
            }
            if (__instance.gameObject.GetComponentInParent<SubRoot>())
            {
                return false;
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
