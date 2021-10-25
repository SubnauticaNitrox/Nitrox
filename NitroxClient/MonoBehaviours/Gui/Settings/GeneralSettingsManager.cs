using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerPreferences;
using UnityEngine.Events;

namespace NitroxClient.MonoBehaviours.Gui.Settings
{
    public class GeneralSettingsManager
    {
        // List of settings which will be in new headings
        public Dictionary<string, List<Setting>> SettingsToAddWithNewHeading;
        // List of settings which will be in already existing headings ("Subtitles", "Display", "Sound", "Advanced")
        public Dictionary<string, List<Setting>> SettingsToAddInAlreadyExistingHeadings;

        public GeneralSettingsManager()
        {
            SettingsToAddWithNewHeading = new Dictionary<string, List<Setting>>();
            SettingsToAddInAlreadyExistingHeadings = new Dictionary<string, List<Setting>>();
            MakeSettings();
        }

        /*
         * Example for adding each type of setting
         * AddSetting("Sound", new Setting(SettingType.TOGGLE, "Test option", true, (UnityAction<bool>)delegate (bool newMode) { }), false);
         * AddSetting("Subtitles", new Setting(SettingType.SLIDER, "TEST SLIDER", 0.2f, (UnityAction<float>)delegate (float newValue) { }, 0f, 1f, 0.5f), false);
         * AddSetting("Advanced", new Setting(SettingType.DROPDOWN, "Test dropdown", (UnityAction<int>)delegate (int newValue) { }, new string[] { "option 1", "option 2", "option 3" }, 0), false);
         */
        private void MakeSettings()
        {
            AddSetting(Language.main.Get("Nitrox_UtilitiesSettings"), new Setting(SettingType.TOGGLE, Language.main.Get("Nitrox_StreamerMode"), NitroxPrefs.StreamerMode, (UnityAction<bool>)delegate (bool newMode) { NitroxPrefs.StreamerMode = newMode; }), true);
        }

        public void AddSetting(string heading, Setting setting, bool newHeading)
        {
            Dictionary<string, List<Setting>> targetDict = newHeading ? SettingsToAddWithNewHeading : SettingsToAddInAlreadyExistingHeadings;
            if (!targetDict.TryGetValue(heading, out List<Setting> settings))
            {
                targetDict.Add(heading, new List<Setting>() { setting });
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

            // base constructor (works for ToggleSetting in which case callback should be a UnityAction<bool>)
            public Setting(SettingType settingType, string label, object value, object callback)
            {
                SettingType = settingType;
                Label = label;
                Value = value;
                Callback = callback;
            }

            // Constructor for SliderSetting, callback should be a UnityAction<float>
            public Setting(SettingType settingType, string label, object value, object callback, float minValue, float maxValue, float defaultValue) : this(settingType, label, value, callback)
            {
                MinValue = minValue;
                MaxValue = maxValue;
                DefaultValue = defaultValue;
            }

            // Constructor for DropdownSetting, callback should be UnityAction<int>
            public Setting(SettingType settingType, string label, object callback, string[] items, int currentIndex) : this(settingType, label, null, callback)
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
