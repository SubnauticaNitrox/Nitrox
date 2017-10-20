using Harmony;
using NitroxClient.MonoBehaviours.Gui.Settings;
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
            SettingsOptions settingsOptions = new SettingsOptions();

            int tabIndex = __instance.AddTab("Multiplayer");
            settingsOptions.AddInputFieldOption(tabIndex, "Name", "Enter Name", SettingsManager.GetName(), __instance, new UnityAction<string>(SettingsManager.SetName));
            settingsOptions.AddKeybindOption(tabIndex, "Chat", SettingsManager.GetKey_Chat(), __instance, new UnityAction<string>(SettingsManager.SetKey_Chat));
            __instance.AddHeading(tabIndex, "Player color");
            __instance.AddSliderOption(tabIndex, "Red", SettingsManager.GetColorR(), 0, new UnityAction<float>(SettingsManager.SetColorR));
            __instance.AddSliderOption(tabIndex, "Green", SettingsManager.GetColorG(), 0, new UnityAction<float>(SettingsManager.SetColorG));
            __instance.AddSliderOption(tabIndex, "Blue", SettingsManager.GetColorB(), 0, new UnityAction<float>(SettingsManager.SetColorB));
            settingsOptions.AddColorImage(tabIndex, "Color", SettingsManager.GetColor_Image(), __instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
