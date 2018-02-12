using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;
using UnityEngine.Events;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_OptionsPanel_AddTabs_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_OptionsPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AddTabs", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(uGUI_OptionsPanel __instance)
        {
            __instance.gameObject.AddComponent<MultiplayerSettings>();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
