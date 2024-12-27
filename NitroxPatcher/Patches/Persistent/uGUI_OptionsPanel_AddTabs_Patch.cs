using System.Collections.Generic;
using System.Reflection;
using NitroxClient.GameLogic.Settings;
using NitroxModel.Helper;
using UnityEngine.Events;

namespace NitroxPatcher.Patches.Persistent
{
    public partial class uGUI_OptionsPanel_AddTabs_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_OptionsPanel t) => t.AddTabs());
        private static NitroxSettingsManager nitroxSettingsManager;

        public static void Postfix(uGUI_OptionsPanel __instance)
        {
            nitroxSettingsManager ??= Resolve<NitroxSettingsManager>(true);
            int tabIndex = __instance.AddTab("Nitrox");
            foreach (KeyValuePair<string, List<NitroxSettingsManager.Setting>> settingEntries in nitroxSettingsManager.NitroxSettings)
            {
                __instance.AddHeading(tabIndex, settingEntries.Key);
                foreach (NitroxSettingsManager.Setting setting in settingEntries.Value)
                {
                    switch (setting.SettingType)
                    {
                        case NitroxSettingsManager.SettingType.TOGGLE:
                            __instance.AddToggleOption(tabIndex, setting.Label, setting.GetValue<bool>(), (UnityAction<bool>)setting.Callback);
                            break;
                        case NitroxSettingsManager.SettingType.SLIDER:
                            __instance.AddSliderOption(tabIndex, setting.Label, setting.GetValue<float>(), setting.SliderMinValue, setting.SliderMaxValue, setting.SliderDefaultValue, setting.SliderStep, (UnityAction<float>)setting.Callback, setting.LabelMode, setting.FloatFormat, setting.Tooltip);
                            break;
                        case NitroxSettingsManager.SettingType.LIST:
                            __instance.AddChoiceOption(tabIndex, setting.Label, setting.ListItems, setting.GetValue<int>(), (UnityAction<int>)setting.Callback);
                            break;
                        case NitroxSettingsManager.SettingType.BUTTON:
                            __instance.AddButton(tabIndex, setting.Label, (UnityAction)setting.Callback);
                            break;
                    }
                }
            }
        }
    }
}
