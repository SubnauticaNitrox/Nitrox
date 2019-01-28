using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public class FireExtinguisher_UseExtinguisher_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(FireExtinguisher);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UseExtinguisher", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(FireExtinguisher __instance, float douseAmount, float expendAmount)
        {
            Fire fire = (Fire)__instance.ReflectionGet("fireTarget");
            if (fire)
            {
                NitroxServiceLocator.LocateService<FireManager>().UpdateFire(fire, douseAmount);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
