using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Fire_Douse_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fire);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Douse", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null);

        public static void Postfix(Fire __instance, float amount)
        {
            if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
            {
                NitroxServiceLocator.LocateService<Fires>().OnDouse(__instance, 10000);
            }
            else
            {
                NitroxServiceLocator.LocateService<Fires>().OnDouse(__instance, amount);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
