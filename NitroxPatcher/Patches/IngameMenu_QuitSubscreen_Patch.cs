using System;
using System.Reflection;
using Harmony;

namespace NitroxPatcher.Patches
{
    public class IngameMenu_QuitSubscreen_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QuitSubscreen");

        public static bool Prefix()
        {
            IngameMenu.main.QuitGame(false);
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
