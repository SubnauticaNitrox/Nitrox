using System;
using System.Diagnostics;
using System.Reflection;
using Harmony;
using UWE;

namespace NitroxPatcher.Patches
{
    public class FreezeTime_Begin_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(FreezeTime);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Begin", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(FreezeTime __instance)
        {
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
