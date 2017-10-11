using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsHelmHUDManager_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHelmHUDManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StopPiloting", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo TARGET_METHOD_Start = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo TARGET_METHOD_Update = TARGET_CLASS.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Start(CyclopsHelmHUDManager __instance)
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

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            __instance.ReflectionSet("hudActive", true);
        }

        public static void Update(CyclopsHelmHUDManager __instance)
        {
            if (!__instance.hornObject.activeSelf && (bool)__instance.ReflectionGet("hudActive"))
            {
                __instance.canvasGroup.interactable = false;
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
            this.PatchPostfix(harmony, TARGET_METHOD_Start, "Start");
            this.PatchPostfix(harmony, TARGET_METHOD_Update, "Update");
        }
    }
}
