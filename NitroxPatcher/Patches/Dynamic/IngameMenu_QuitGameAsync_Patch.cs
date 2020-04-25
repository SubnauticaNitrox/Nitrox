using System.Reflection;
using Harmony;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_QuitGameAsync_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(IngameMenu).GetMethod("QuitGameAsync");

        public static bool Prefix(bool quitToDesktop)
        {
            if (!quitToDesktop)
            {
                UWE.Utils.lockCursor = false;
            }
            else
            {
                Application.Quit();
            }
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
