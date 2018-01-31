using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_MainMenu_Start_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_MainMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(uGUI_MainMenu __instance)
        {
            // If the player starts the main menu for the first time, or returns from a (multiplayer) session, get rid of all the patches if applicable.
            Main.Restore();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
