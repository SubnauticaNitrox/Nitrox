using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Both owners and non-owners will be able to douse fires on a ship.
    /// </summary>
    public class Fire_Douse_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fire);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Douse", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null);

        public static void Postfix(Fire __instance, float amount)
        {
            if (!__instance.livemixin.IsAlive() && !__instance.IsExtinguished())
            {
                Multiplayer.Logic.Cyclops.OnFireDoused(__instance,__instance.fireSubRoot, 10000);
            }
            else
            {
                Multiplayer.Logic.Cyclops.OnFireDoused(__instance, __instance.fireSubRoot, amount);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
