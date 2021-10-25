using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerPreferences;
using UnityEngine.Events;

namespace NitroxClient.MonoBehaviours.Gui.Settings
{
    public class GeneralSettingsManager
    {
        public Dictionary<string, List<Setting>> SettingsToAdd;
        public GeneralSettingsManager()
        {
            SettingsToAdd = new Dictionary<string, List<Setting>>()
            {
                { "Utilities", new List<Setting>()
                    {
                        new Setting(SettingType.TOGGLE, "Streamer mode", NitroxPrefs.StreamerMode, (UnityAction<bool>)delegate (bool newMode) { NitroxPrefs.StreamerMode = newMode; })
                    }
                }
            };
        }

        public void AddSetting(string heading, Setting setting)
        {
            if (!SettingsToAdd.TryGetValue(heading, out List<Setting> settings))
            {
                SettingsToAdd.Add(heading, new List<Setting>() { setting });
            }
            else
            {
                settings.Add(setting);
            }
        }

        public class Setting
        {
            public SettingType SettingType;
            public string Label;
            public object Value;
            public object Callback;

            // Slider specifics
            public float MinValue;
            public float MaxValue;
            public float DefaultValue;

            // Dropdown specifics
            public string[] Items;
            public int CurrentIndex;

            // base constructor (works for ToggleSetting)
            public Setting(SettingType settingType, string label, object value, object callback)
            {
                SettingType = settingType;
                Label = label;
                Value = value;
                Callback = callback;
            }

            // Constructor for SliderSetting
            public Setting(SettingType settingType, string label, object value, object callback, float minValue, float maxValue, float defaultValue) : this(settingType, label, value, callback)
            {
                MinValue = minValue;
                MaxValue = maxValue;
                DefaultValue = defaultValue;
            }

            // Constructor for DropdownSetting
            public Setting(SettingType settingType, string label, object value, object callback, string[] items, int currentIndex) : this(settingType, label, value, callback)
            {
                Items = items;
                CurrentIndex = currentIndex;
            }
        }

        public enum SettingType
        {
            TOGGLE, SLIDER, DROPDOWN
        }
    }
}
