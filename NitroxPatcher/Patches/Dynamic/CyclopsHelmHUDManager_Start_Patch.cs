using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsHelmHUDManager_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHelmHUDManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            __instance.ReflectionSet("hudActive", true);
            if (__instance.motorMode.engineOn)
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOn");
            }
            else
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOff");
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
