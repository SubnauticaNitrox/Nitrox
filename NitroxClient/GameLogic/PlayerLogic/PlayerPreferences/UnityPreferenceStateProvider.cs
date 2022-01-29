using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences
{
    /// <summary>
    ///     This abstraction allows us to write tests against the preferences manager. Otherwise - we are unduly tied to Unity.
    /// </summary>
    public class UnityPreferenceStateProvider : IPreferenceStateProvider
    {
        private const string UNITY_PREF_KEY_NAME = "NITROX_PLAYER_PREFS";

        public PlayerPreferenceState GetPreferenceState()
        {
            JsonMapper.RegisterImporter((double value) => Convert.ToSingle(value));
            JsonMapper.RegisterExporter<float>((value, writer) => writer.Write(Convert.ToDouble(value)));

            string playerPreferencesJson = PlayerPrefs.GetString(UNITY_PREF_KEY_NAME);

            if (string.IsNullOrEmpty(playerPreferencesJson) || playerPreferencesJson == "{}")
            {
                return new PlayerPreferenceState
                {
                    Preferences = new Dictionary<string, PlayerPreference>()
                };
            }

            return JsonMapper.ToObject<PlayerPreferenceState>(playerPreferencesJson);
        }

        public void SavePreferenceState(PlayerPreferenceState preferenceState)
        {
            JsonMapper.RegisterImporter((double value) => Convert.ToSingle(value));
            JsonMapper.RegisterExporter<float>((value, writer) => writer.Write(Convert.ToDouble(value)));

            string playerPreferencesJson = JsonMapper.ToJson(preferenceState);
            PlayerPrefs.SetString(UNITY_PREF_KEY_NAME, playerPreferencesJson);
        }
    }
}
