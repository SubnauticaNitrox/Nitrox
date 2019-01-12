using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Both owners and non-owners can douse fires on a ship.
    /// </summary>
    public class Fire_Douse_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fire);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Douse", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null);

        public static void Postfix(Fire __instance, float amount)
        {
            SubRoot subRoot = __instance.gameObject.RequireComponentInParent<SubRoot>();

            if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnFireDoused(__instance, __instance.fireSubRoot, 10000);
            }
            else
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnFireDoused(__instance, __instance.fireSubRoot, amount);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
