using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class IngameMenu_QuitGameAsync_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.QuitGameAsync(default(bool)));

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

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
