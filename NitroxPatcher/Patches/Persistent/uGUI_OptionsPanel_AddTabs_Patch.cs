using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Settings;
using NitroxModel.Helper;
using UnityEngine.Events;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_OptionsPanel_AddTabs_Patch : NitroxPatch, IPersistentPatch
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
                            __instance.AddSliderOption(tabIndex, setting.Label, setting.GetValue<float>(), setting.SliderMinValue, setting.SliderMaxValue, setting.SliderDefaultValue, (UnityAction<float>)setting.Callback);
                            break;
                        case NitroxSettingsManager.SettingType.LIST:
                            __instance.AddChoiceOption(tabIndex, setting.Label, setting.ListItems, setting.GetValue<int>(), (UnityAction<int>)setting.Callback);
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

