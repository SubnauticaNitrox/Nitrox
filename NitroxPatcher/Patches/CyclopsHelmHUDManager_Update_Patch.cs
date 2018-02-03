using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    class CyclopsHelmHUDManager_Update_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHelmHUDManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            //To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
            if (!__instance.hornObject.activeSelf && (bool)__instance.ReflectionGet("hudActive"))
            {
                __instance.canvasGroup.interactable = false;
            }
            else if (!(bool)__instance.ReflectionGet("hudActive"))
            {
                __instance.ReflectionSet("hudActive", true);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
