using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.GameLogic.Settings;
using UnityEngine.Events;

namespace NitroxClient.Patching.Patches.Persistent;

internal partial class uGUI_OptionsPanel_AddTabs_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_OptionsPanel t) => t.AddTabs());
    private static NitroxSettingsService nitroxSettingsService;

    public uGUI_OptionsPanel_AddTabs_Patch(NitroxSettingsService nsm)
    {
        nitroxSettingsService = nsm ?? throw new ArgumentNullException(nameof(nsm));
    }

    public static void Postfix(uGUI_OptionsPanel __instance)
    {
        int tabIndex = __instance.AddTab("Nitrox");
        foreach (KeyValuePair<string, List<NitroxSettingsService.Setting>> settingEntries in nitroxSettingsService.NitroxSettings)
        {
            __instance.AddHeading(tabIndex, settingEntries.Key);
            foreach (NitroxSettingsService.Setting setting in settingEntries.Value)
            {
                switch (setting.SettingType)
                {
                    case NitroxSettingsService.SettingType.TOGGLE:
                        __instance.AddToggleOption(tabIndex, setting.Label, setting.GetValue<bool>(), (UnityAction<bool>)setting.Callback);
                        break;
                    case NitroxSettingsService.SettingType.SLIDER:
                        __instance.AddSliderOption(tabIndex, setting.Label, setting.GetValue<float>(), setting.SliderMinValue, setting.SliderMaxValue, setting.SliderDefaultValue, setting.SliderStep, (UnityAction<float>)setting.Callback, setting.LabelMode,
                                                   setting.FloatFormat, setting.Tooltip);
                        break;
                    case NitroxSettingsService.SettingType.LIST:
                        __instance.AddChoiceOption(tabIndex, setting.Label, setting.ListItems, setting.GetValue<int>(), (UnityAction<int>)setting.Callback);
                        break;
                    case NitroxSettingsService.SettingType.BUTTON:
                        __instance.AddButton(tabIndex, setting.Label, (UnityAction)setting.Callback);
                        break;
                }
            }
        }
    }
}
