using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Settings;
using NitroxModel.Helper;
using UnityEngine.Events;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_TabbedControlsPanel_AddHeading_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_TabbedControlsPanel t) => t.AddHeading(default(int), default(string)));

        public static void Postfix(uGUI_OptionsPanel __instance, int tabIndex, string label)
        {
            GeneralSettingsManager generalSettingsManager = new();
            if (generalSettingsManager.SettingsToAddInAlreadyExistingHeadings.TryGetValue(label, out List<GeneralSettingsManager.Setting> settingsList))
            {
                foreach (GeneralSettingsManager.Setting setting in settingsList)
                {
                    switch (setting.SettingType)
                    {
                        case GeneralSettingsManager.SettingType.TOGGLE:
                            __instance.AddToggleOption(tabIndex, setting.Label, (bool)setting.Value, (UnityAction<bool>)setting.Callback);
                            break;
                        case GeneralSettingsManager.SettingType.SLIDER:
                            __instance.AddSliderOption(tabIndex, setting.Label, (float)setting.Value, setting.MinValue, setting.MaxValue, setting.DefaultValue, (UnityAction<float>)setting.Callback);
                            break;
                        case GeneralSettingsManager.SettingType.DROPDOWN:
                            __instance.AddChoiceOption(tabIndex, setting.Label, setting.Items, setting.CurrentIndex, (UnityAction<int>)setting.Callback);
                            break;
                    }
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
