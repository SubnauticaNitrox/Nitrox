using System;
using NitroxClient.GameLogic.Settings;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class NitroxPrefs
    {
        // Add new fields here, there are 2 examples for the different types
        // public static NitroxPref ListPref = new NitroxPref("Nitrox.dropdownPref", 1, NitroxSettingsManager.SettingType.LIST);
        // public static NitroxPref SliderPref = new NitroxPref("Nitrox.sliderPref", 0.4f, NitroxSettingsManager.SettingType.SLIDER);
        // It's very important to enter the exact types you need else you will have an incomprehensible error
        // In the example above, ListPref's default value is an int and SliderPref's one is a float
        public static readonly NitroxPref<bool> HideIpPref = new ("Nitrox.hideIp", NitroxSettingsManager.SettingType.TOGGLE);
        public static readonly NitroxPref<bool> SilenceChatPref = new ("Nitrox.silenceChat", NitroxSettingsManager.SettingType.TOGGLE);
    }

    public class NitroxPref<T> : NitroxPref
    {
        public new T Value
        {
            get => (T)base.Value;
            set => base.Value = value;
        }
        public new T DefaultValue => (T)base.DefaultValue;

        // When not specifying the defaultValue, it will take the default value of the type (e.g: for a bool, default is false)
        public NitroxPref(string key, NitroxSettingsManager.SettingType settingType, T defaultValue = default) : base(key, settingType, defaultValue)
        {
        }
    }

    public abstract class NitroxPref
    {
        public string Key { get; init; }
        public object Value
        {
            get {
                switch (SettingType)
                {
                    case NitroxSettingsManager.SettingType.TOGGLE:
                        return PlayerPrefs.GetInt(Key, (bool)DefaultValue ? 1 : 0) == 1;
                    case NitroxSettingsManager.SettingType.SLIDER:
                        return PlayerPrefs.GetFloat(Key, (float)DefaultValue);
                    case NitroxSettingsManager.SettingType.LIST:
                        return PlayerPrefs.GetInt(Key, (int)DefaultValue);
                    default:
                        return null;
                }
            }
            set
            {
                switch (SettingType)
                {
                    case NitroxSettingsManager.SettingType.TOGGLE:
                        PlayerPrefs.SetInt(Key, Convert.ToBoolean(value) ? 1 : 0);
                        break;
                    case NitroxSettingsManager.SettingType.SLIDER:
                        PlayerPrefs.SetFloat(Key, Convert.ToSingle(value));
                        break;
                    case NitroxSettingsManager.SettingType.LIST:
                        PlayerPrefs.SetInt(Key, Convert.ToInt32(value));
                        break;
                }
                PlayerPrefs.Save();
            }
        }

        public object DefaultValue { get; init; }
        public NitroxSettingsManager.SettingType SettingType { get; init; }

        public NitroxPref(string key, NitroxSettingsManager.SettingType settingType, object defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            SettingType = settingType;
        }
    }
}
