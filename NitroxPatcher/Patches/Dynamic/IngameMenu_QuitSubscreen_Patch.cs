using System;
using System.Reflection;
using Harmony;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_QuitSubscreen_Patch : NitroxPatch, IDynamicPatch
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
