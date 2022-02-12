using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine.Events;

namespace NitroxClient.GameLogic.Settings
{
    public class NitroxSettingsManager
    {
        // Lists of settings sorted by headings
        public readonly Dictionary<string, List<Setting>> NitroxSettings;

        public NitroxSettingsManager()
        {
            NitroxSettings = new Dictionary<string, List<Setting>>();
            MakeSettings();
        }

        // This is where you should add new settings, there are 3 types of available:
        // TOGGLE (bool type), SLIDER (float type), LIST (int type)
        private void MakeSettings()
        {
            // Examples:
            // AddSetting("Subtitles", new Setting("Test Slidder", NitroxPrefs.SliderPref, newValue => NitroxPrefs.SliderPref.Value = newValue, 0.1f, 1f, 0.4f));
            // AddSetting("Advanced", new Setting("Test list", NitroxPrefs.ListPref, newIndex => NitroxPrefs.ListPref.Value = newIndex, new string[] { "option 1", "option 2", "option 3" }));
            AddSetting("Nitrox_StreamerSettings", new Setting("Nitrox_SilenceChat", NitroxPrefs.SilenceChat, silence => NitroxPrefs.SilenceChat.Value = silence));
            AddSetting("Nitrox_StreamerSettings", new Setting("Nitrox_HideIp", NitroxPrefs.HideIp, hide =>
            {
                NitroxPrefs.HideIp.Value = hide;
                MainMenuMultiplayerPanel.Main.RefreshServerEntries();
            }));
        }

        /// <summary>Adds a setting to the list under a certain heading</summary>
        public void AddSetting(string heading, Setting setting)
        {
            if (NitroxSettings.TryGetValue(heading, out List<Setting> settings))
            {
                settings.Add(setting);
            }
            else
            {
                NitroxSettings.Add(heading, new List<Setting> { setting });
            }
        }

        public class Setting
        {
            // These fields are used by each type of setting
            // To get the value, you need to type setting.GetValue<type>() or (type)NitroxPrefs.MyPref.Value when you don't have the setting
            public readonly SettingType SettingType;
            public readonly string Label;
            public readonly NitroxPref NitroxPref;
            public readonly Delegate Callback;

            // Slider specifics
            public readonly float SliderMinValue;
            public readonly float SliderMaxValue;
            public readonly float SliderDefaultValue;

            // List specifics
            public readonly string[] ListItems;

            /// <summary>Base constructor for the class</summary>
            private Setting(SettingType settingType, string label, NitroxPref nitroxPref, Delegate callback)
            {
                SettingType = settingType;
                Label = label;
                NitroxPref = nitroxPref;
                Callback = callback;
            }

            /// <summary>Constructor for a Toggle setting</summary>
            public Setting(string label, NitroxPref nitroxPref, UnityAction<bool> callback) : this(SettingType.TOGGLE, label, nitroxPref, callback) { }

            /// <summary>Constructor for a Slider setting</summary>
            public Setting(string label, NitroxPref nitroxPref, UnityAction<float> callback, float sliderMinValue, float sliderMaxValue, float sliderDefaultValue) : this(SettingType.SLIDER, label, nitroxPref, callback)
            {
                SliderMinValue = sliderMinValue;
                SliderMaxValue = sliderMaxValue;
                SliderDefaultValue = sliderDefaultValue;
            }

            /// <summary>Constructor for a List setting</summary>
            public Setting(string label, NitroxPref nitroxPref, UnityAction<int> callback, string[] listItems) : this(SettingType.LIST, label, nitroxPref, callback)
            {
                ListItems = listItems;
            }

            public T GetValue<T>() where T : IConvertible
            {
                return ((NitroxPref<T>)NitroxPref).Value;
            }
        }

        public enum SettingType
        {
            TOGGLE, SLIDER, LIST
        }
    }
}
