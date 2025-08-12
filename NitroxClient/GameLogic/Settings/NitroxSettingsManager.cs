using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using UnityEngine.Events;

namespace NitroxClient.GameLogic.Settings;

public class NitroxSettingsManager
{
    /// <summary>
    /// Settings grouped by their headings
    /// </summary>
    public readonly Dictionary<string, List<Setting>> NitroxSettings;

    public NitroxSettingsManager()
    {
        NitroxSettings = new Dictionary<string, List<Setting>>();
        MakeSettings();
    }

    /// <summary>
    /// Allows to create new settings
    ///
    /// Available types : TOGGLE, SLIDER, LIST, BUTTON
    ///
    /// <example>
    /// <para>Examples :</para>
    /// <code>
    ///  AddSetting("Subtitles", new Setting("Test Slidder", NitroxPrefs.SliderPref, newValue => NitroxPrefs.SliderPref.Value = newValue, 0.1f, 1f, 0.4f));
    ///  AddSetting("Advanced", new Setting("Test list", NitroxPrefs.ListPref, newIndex => NitroxPrefs.ListPref.Value = newIndex, new string[] { "option 1", "option 2", "option 3" }));
    /// </code>
    /// </example>
    /// </summary>
    private void MakeSettings()
    {
        AddSetting("Nitrox_StreamerSettings", new Setting("Nitrox_SilenceChat", NitroxPrefs.SilenceChat, silence => NitroxPrefs.SilenceChat.Value = silence));
        AddSetting("Nitrox_StreamerSettings", new Setting("Nitrox_HideIp", NitroxPrefs.HideIp, hide =>
        {
            NitroxPrefs.HideIp.Value = hide;
            MainMenuServerListPanel.Main.RefreshServerEntries();
        }));

        AddSetting("Nitrox_ResyncSettings", new Setting("Nitrox_ResyncBuildings", () =>
        {
            if (BuildingHandler.Main)
            {
                BuildingHandler.Main.AskForResync();
            }
        }));

        AddSetting("Nitrox_BuildingSettings", new Setting("Nitrox_SafeBuilding", NitroxPrefs.SafeBuilding, safe => NitroxPrefs.SafeBuilding.Value = safe));
        AddSetting("Nitrox_BuildingSettings", new Setting("Nitrox_SafeBuildingLog", NitroxPrefs.SafeBuildingLog, safeLog => NitroxPrefs.SafeBuildingLog.Value = safeLog));

        AddSetting("Nitrox_Settings_Bandwidth", new Setting("Nitrox_Settings_LatencyUpdatePeriod", NitroxPrefs.LatencyUpdatePeriod, latencyUpdatePeriod => NitroxPrefs.LatencyUpdatePeriod.Value = (int)latencyUpdatePeriod, 1, 60, NitroxPrefs.LatencyUpdatePeriod.DefaultValue, 1, SliderLabelMode.Int, tooltip: "Nitrox_Settings_HigherForUnstable_Tooltip"));
        AddSetting("Nitrox_Settings_Bandwidth", new Setting("Nitrox_Settings_SafetyLatencyMargin", NitroxPrefs.SafetyLatencyMargin, safetyLatencyMargin => NitroxPrefs.SafetyLatencyMargin.Value = safetyLatencyMargin, 0.01f, 0.5f, NitroxPrefs.SafetyLatencyMargin.DefaultValue, 0.01f, SliderLabelMode.Float, "0.00", "Nitrox_Settings_HigherForUnstable_Tooltip"));
        AddSetting("Nitrox_Settings_Bandwidth", new Setting("Nitrox_Settings_OfflineClockSyncDuration", NitroxPrefs.OfflineClockSyncDuration, offlineClockSyncDuration => NitroxPrefs.OfflineClockSyncDuration.Value = (int)offlineClockSyncDuration, 3, 15, NitroxPrefs.OfflineClockSyncDuration.DefaultValue, 1, SliderLabelMode.Int, tooltip: "Nitrox_Settings_HigherForUnstable_Tooltip"));
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
        public readonly float SliderStep;
        public readonly SliderLabelMode LabelMode;
        /// <summary>
        /// Examples: "0", "0.00"
        /// </summary>
        public string FloatFormat;
        public readonly string Tooltip;

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

        /// <summary>Constructor for buttons (doesn't need a NitroxPref)</summary>
        public Setting(string label, UnityAction callback)
        {
            SettingType = SettingType.BUTTON;
            Label = label;
            Callback = callback;
        }

        /// <summary>Constructor for a Toggle setting</summary>
        public Setting(string label, NitroxPref nitroxPref, UnityAction<bool> callback) : this(SettingType.TOGGLE, label, nitroxPref, callback) { }

        /// <summary>Constructor for a Slider setting</summary>
        public Setting(string label, NitroxPref nitroxPref, UnityAction<float> callback, float sliderMinValue, float sliderMaxValue, float sliderDefaultValue, float sliderStep, SliderLabelMode labelMode, string floatFormat = "0", string tooltip = null) : this(SettingType.SLIDER, label, nitroxPref, callback)
        {
            SliderMinValue = sliderMinValue;
            SliderMaxValue = sliderMaxValue;
            SliderDefaultValue = sliderDefaultValue;
            SliderStep = sliderStep;
            LabelMode = labelMode;
            FloatFormat = floatFormat;
            Tooltip = tooltip;
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
        TOGGLE, SLIDER, LIST, BUTTON
    }
}
