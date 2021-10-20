using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsHelmHUDManager_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Start());

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            __instance.hudActive = true;
            __instance.engineToggleAnimator.SetTrigger(__instance.motorMode.engineOn ? "EngineOn" : "EngineOff");
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
