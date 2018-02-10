using System;
using System.Reflection;
using Harmony;
using UWE;

namespace NitroxPatcher.Patches
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnSelect", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(IngameMenu __instance)
        {
            FreezeTime.End("IngameMenu");
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
