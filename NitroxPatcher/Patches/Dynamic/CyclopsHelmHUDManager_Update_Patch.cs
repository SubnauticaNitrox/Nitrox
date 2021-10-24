using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsHelmHUDManager_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Update());

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            // To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
            if (!__instance.hornObject.activeSelf && __instance.hudActive)
            {
                __instance.canvasGroup.interactable = false;
            }
            else if (!__instance.hudActive)
            {
                __instance.hudActive = true;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
