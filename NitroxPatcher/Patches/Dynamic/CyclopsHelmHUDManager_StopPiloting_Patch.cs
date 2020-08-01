using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsHelmHUDManager_StopPiloting_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHelmHUDManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StopPiloting", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            __instance.ReflectionSet("hudActive", true);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
