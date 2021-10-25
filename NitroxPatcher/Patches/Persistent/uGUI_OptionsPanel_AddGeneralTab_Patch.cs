using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Settings;
using NitroxModel.Helper;
using UnityEngine.Events;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_OptionsPanel_AddGeneralTab_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_OptionsPanel t) => t.AddGeneralTab());

        public static void Postfix(uGUI_OptionsPanel __instance)
        {
            GeneralSettingsManager generalSettingsManager = new();
            int tabIndex = 0;
            foreach (KeyValuePair<string, List<GeneralSettingsManager.Setting>> settingEntry in generalSettingsManager.SettingsToAdd)
            {
                __instance.AddHeading(tabIndex, settingEntry.Key);
                foreach (GeneralSettingsManager.Setting setting in settingEntry.Value)
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
