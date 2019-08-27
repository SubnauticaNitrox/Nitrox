using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NitroxModel.Logger;
using Harmony;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    class IngameMenu_QuitGameAsync_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QuitGameAsync");

        public static bool Prefix(bool quitToDesktop)
        {
            if (!quitToDesktop)
            {
                UWE.Utils.lockCursor = false;
                SceneCleaner.Open();
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
