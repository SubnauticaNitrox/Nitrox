using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitClawArm_TryUse_Patch : NitroxPatch, IDynamicPatch
    {
        /// <summary>
        ///     Unable to use <see cref="Reflect.Method" /> here because expression trees do not support out parameters (yet).
        /// </summary>
        private static readonly MethodInfo TARGET_METHOD = typeof(ExosuitClawArm).GetMethod(nameof(ExosuitClawArm.TryUse), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(float).MakeByRefType() }, null);

        public static void Postfix(bool __result, ExosuitClawArm __instance, float ___cooldownTime)
        {
            if (__result)
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
