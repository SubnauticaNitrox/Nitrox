using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    class ExosuitClawArm_TryUse_Patch : NitroxPatch
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
