using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class uGUI_MainMenu_Start_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_MainMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(uGUI_MainMenu __instance)
        {
            new GameObject().AddComponent<MultiplayerButton>();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
