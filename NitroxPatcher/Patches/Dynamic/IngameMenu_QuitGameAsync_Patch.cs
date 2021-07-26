using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class IngameMenu_QuitGameAsync_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QuitGameAsync");

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
