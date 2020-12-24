using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitClawArm_TryUse_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitClawArm);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryUse", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(bool __result, ExosuitClawArm __instance,float ___cooldownTime)
        {
            if(__result)
            {
                NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastClawUse(__instance, ___cooldownTime);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
