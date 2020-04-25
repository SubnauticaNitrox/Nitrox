using System.Reflection;
using Harmony;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsHelmHUDManager_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsHelmHUDManager).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
