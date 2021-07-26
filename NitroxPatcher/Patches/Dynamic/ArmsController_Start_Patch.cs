using System;
using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ArmsController_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ArmsController);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo reconfigure = TARGET_CLASS.GetMethod("Reconfigure", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ArmsController __instance)
        {
            reconfigure.Invoke(__instance, new PlayerTool[] { null });
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
